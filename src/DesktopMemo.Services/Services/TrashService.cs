using System;
using System.Collections.Generic;
using System.Linq;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Data.Persistence;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.Services.Services;

public class TrashService : ITrashService
{
    private const int RetentionDays = 90;
    private readonly IDbContextFactory _contextFactory;
    private readonly IGroupService _groupService;

    public TrashService(IDbContextFactory contextFactory, IGroupService groupService)
    {
        _contextFactory = contextFactory;
        _groupService = groupService;
    }

    public IList<TrashItemDto> GetTrashItems()
    {
        using (var context = _contextFactory.Create())
        {
            var groups = context.MemoGroups
                .Where(x => x.IsDeleted)
                .Select(x => new TrashItemDto
                {
                    Id = x.Id,
                    ItemType = TrashItemType.Group,
                    Name = x.Name,
                    DeletedAtUtc = x.DeletedAtUtc
                })
                .ToList();

            var notes = context.Notes
                .Where(x => x.IsDeleted)
                .Select(x => new TrashItemDto
                {
                    Id = x.Id,
                    ItemType = TrashItemType.Note,
                    Name = x.Title,
                    DeletedAtUtc = x.DeletedAtUtc
                })
                .ToList();

            return groups
                .Concat(notes)
                .OrderByDescending(x => x.DeletedAtUtc)
                .ToList();
        }
    }

    public void RestoreNote(Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && x.IsDeleted);
            if (note == null)
            {
                return;
            }

            var targetGroup = context.MemoGroups.FirstOrDefault(x => x.Id == note.GroupId && !x.IsDeleted);
            if (targetGroup == null)
            {
                note.GroupId = _groupService.GetOrCreateInboxGroupId();
            }

            note.IsDeleted = false;
            note.DeletedAtUtc = null;
            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void RestoreGroup(Guid groupId)
    {
        using (var context = _contextFactory.Create())
        {
            var group = context.MemoGroups.FirstOrDefault(x => x.Id == groupId && x.IsDeleted);
            if (group == null)
            {
                return;
            }

            if (group.ParentGroupId.HasValue)
            {
                var parent = context.MemoGroups.FirstOrDefault(x => x.Id == group.ParentGroupId.Value && !x.IsDeleted);
                if (parent == null)
                {
                    group.ParentGroupId = null;
                }
            }

            group.IsDeleted = false;
            group.DeletedAtUtc = null;
            group.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void DeleteItemPermanently(Guid itemId, TrashItemType itemType)
    {
        using (var context = _contextFactory.Create())
        {
            if (itemType == TrashItemType.Note)
            {
                var note = context.Notes.FirstOrDefault(x => x.Id == itemId && x.IsDeleted);
                if (note == null)
                {
                    return;
                }

                var state = context.StickyWindowStates.FirstOrDefault(x => x.NoteId == note.Id);
                if (state != null)
                {
                    context.StickyWindowStates.Remove(state);
                }

                context.Notes.Remove(note);
                context.SaveChanges();
                return;
            }

            var root = context.MemoGroups.FirstOrDefault(x => x.Id == itemId && x.IsDeleted);
            if (root == null)
            {
                return;
            }

            var allGroups = context.MemoGroups.ToList();
            var descendantIds = CollectDescendantGroupIds(allGroups, root.Id);
            if (descendantIds.Length == 0)
            {
                return;
            }

            var hasActiveGroups = context.MemoGroups.Any(x => descendantIds.Contains(x.Id) && !x.IsDeleted);
            var hasActiveNotes = context.Notes.Any(x => descendantIds.Contains(x.GroupId) && !x.IsDeleted);
            if (hasActiveGroups || hasActiveNotes)
            {
                throw new InvalidOperationException("삭제 대상 하위에 활성 항목이 있어 영구 삭제할 수 없습니다.");
            }

            var notesToDelete = context.Notes.Where(x => descendantIds.Contains(x.GroupId) && x.IsDeleted).ToList();
            if (notesToDelete.Count > 0)
            {
                var noteIds = notesToDelete.Select(x => x.Id).ToList();
                var states = context.StickyWindowStates.Where(x => noteIds.Contains(x.NoteId)).ToList();
                context.StickyWindowStates.RemoveRange(states);
                context.Notes.RemoveRange(notesToDelete);
            }

            var groupsToDelete = context.MemoGroups.Where(x => descendantIds.Contains(x.Id) && x.IsDeleted).ToList();
            RemoveGroupsLeafFirst(context, groupsToDelete.Select(x => x.Id).ToList());
            context.SaveChanges();
        }
    }

    public void PurgeExpiredItems()
    {
        var threshold = DateTime.UtcNow.AddDays(-RetentionDays);
        using (var context = _contextFactory.Create())
        {
            var notesToDelete = context.Notes.Where(x => x.IsDeleted && x.DeletedAtUtc <= threshold).ToList();
            if (notesToDelete.Count > 0)
            {
                var noteIds = notesToDelete.Select(x => x.Id).ToList();
                var states = context.StickyWindowStates.Where(x => noteIds.Contains(x.NoteId)).ToList();
                context.StickyWindowStates.RemoveRange(states);
                context.Notes.RemoveRange(notesToDelete);
            }

            var groupsToDelete = context.MemoGroups.Where(x => x.IsDeleted && x.DeletedAtUtc <= threshold).ToList();
            RemoveGroupsLeafFirst(context, groupsToDelete.Select(x => x.Id).ToList());

            context.SaveChanges();
        }
    }

    public void EmptyTrash()
    {
        using (var context = _contextFactory.Create())
        {
            var notesToDelete = context.Notes.Where(x => x.IsDeleted).ToList();
            if (notesToDelete.Count > 0)
            {
                var noteIds = notesToDelete.Select(x => x.Id).ToList();
                var states = context.StickyWindowStates.Where(x => noteIds.Contains(x.NoteId)).ToList();
                context.StickyWindowStates.RemoveRange(states);
                context.Notes.RemoveRange(notesToDelete);
            }

            var groupsToDelete = context.MemoGroups.Where(x => x.IsDeleted).ToList();
            RemoveGroupsLeafFirst(context, groupsToDelete.Select(x => x.Id).ToList());

            context.SaveChanges();
        }
    }

    private static Guid[] CollectDescendantGroupIds(IList<DesktopMemo.Domain.Entities.MemoGroup> groups, Guid rootId)
    {
        var collected = new HashSet<Guid> { rootId };
        var queue = new Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = groups.Where(x => x.ParentGroupId == current).Select(x => x.Id).ToList();
            foreach (var child in children)
            {
                if (!collected.Add(child))
                {
                    continue;
                }

                queue.Enqueue(child);
            }
        }

        return collected.ToArray();
    }

    private static void RemoveGroupsLeafFirst(DesktopMemoDbContext context, IList<Guid> candidateIds)
    {
        var remaining = context.MemoGroups.Where(x => candidateIds.Contains(x.Id)).ToList();

        while (remaining.Count > 0)
        {
            var remainingIds = remaining.Select(x => x.Id).ToHashSet();
            var leaves = remaining.Where(x => !remainingIds.Contains(x.ParentGroupId ?? Guid.Empty)).ToList();
            if (leaves.Count == 0)
            {
                break;
            }

            context.MemoGroups.RemoveRange(leaves);
            foreach (var leaf in leaves)
            {
                remaining.Remove(leaf);
            }
        }
    }
}
