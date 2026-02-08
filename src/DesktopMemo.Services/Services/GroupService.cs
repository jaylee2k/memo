using System;
using System.Linq;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Domain.Requests;
using DesktopMemo.Services.Mapping;

namespace DesktopMemo.Services.Services;

public class GroupService : IGroupService
{
    private readonly IDbContextFactory _contextFactory;

    public GroupService(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public GroupTreeNodeDto CreateGroup(CreateGroupRequest req)
    {
        if (req == null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        using (var context = _contextFactory.Create())
        {
            if (req.ParentGroupId.HasValue)
            {
                var parent = context.MemoGroups.FirstOrDefault(x => x.Id == req.ParentGroupId.Value && !x.IsDeleted);
                if (parent == null)
                {
                    throw new InvalidOperationException("상위 그룹을 찾을 수 없습니다.");
                }
            }

            var siblingCount = context.MemoGroups.Count(x => x.ParentGroupId == req.ParentGroupId && !x.IsDeleted);
            var now = DateTime.UtcNow;

            var group = new MemoGroup
            {
                Id = Guid.NewGuid(),
                ParentGroupId = req.ParentGroupId,
                Name = string.IsNullOrWhiteSpace(req.Name) ? "새 그룹" : req.Name.Trim(),
                SortOrder = siblingCount,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.MemoGroups.Add(group);
            context.SaveChanges();

            return EntityMapper.ToNode(group);
        }
    }

    public GroupTreeNodeDto UpdateGroup(UpdateGroupRequest req)
    {
        if (req == null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        using (var context = _contextFactory.Create())
        {
            var group = context.MemoGroups.FirstOrDefault(x => x.Id == req.Id && !x.IsDeleted);
            if (group == null)
            {
                throw new InvalidOperationException("그룹을 찾을 수 없습니다.");
            }

            group.Name = string.IsNullOrWhiteSpace(req.Name) ? group.Name : req.Name.Trim();
            group.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();

            return EntityMapper.ToNode(group);
        }
    }

    public void SoftDeleteGroup(Guid groupId)
    {
        using (var context = _contextFactory.Create())
        {
            var root = context.MemoGroups.FirstOrDefault(x => x.Id == groupId && !x.IsDeleted);
            if (root == null)
            {
                return;
            }

            var allIds = EntityMapper.CollectDescendantGroupIds(context.MemoGroups.Where(x => !x.IsDeleted), groupId);
            var now = DateTime.UtcNow;

            var groups = context.MemoGroups.Where(x => allIds.Contains(x.Id)).ToList();
            foreach (var group in groups)
            {
                group.IsDeleted = true;
                group.DeletedAtUtc = now;
                group.UpdatedAtUtc = now;
            }

            var notes = context.Notes.Where(x => allIds.Contains(x.GroupId) && !x.IsDeleted).ToList();
            foreach (var note in notes)
            {
                note.IsDeleted = true;
                note.DeletedAtUtc = now;
                note.UpdatedAtUtc = now;
            }

            context.SaveChanges();
        }
    }

    public System.Collections.Generic.IList<GroupTreeNodeDto> GetGroupTree()
    {
        using (var context = _contextFactory.Create())
        {
            var groups = context.MemoGroups
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToList();

            var dict = groups.ToDictionary(x => x.Id, EntityMapper.ToNode);
            foreach (var group in groups)
            {
                if (group.ParentGroupId.HasValue && dict.ContainsKey(group.ParentGroupId.Value))
                {
                    dict[group.ParentGroupId.Value].Children.Add(dict[group.Id]);
                }
            }

            return groups
                .Where(x => !x.ParentGroupId.HasValue || !dict.ContainsKey(x.ParentGroupId.Value))
                .Select(x => dict[x.Id])
                .ToList();
        }
    }

    public Guid GetOrCreateInboxGroupId()
    {
        using (var context = _contextFactory.Create())
        {
            var inbox = context.MemoGroups.FirstOrDefault(x => !x.IsDeleted && x.Name == "Inbox");
            if (inbox != null)
            {
                return inbox.Id;
            }

            var now = DateTime.UtcNow;
            inbox = new MemoGroup
            {
                Id = Guid.NewGuid(),
                Name = "Inbox",
                SortOrder = 0,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.MemoGroups.Add(inbox);
            context.SaveChanges();

            return inbox.Id;
        }
    }
}
