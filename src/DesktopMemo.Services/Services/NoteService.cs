using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Domain.Requests;
using DesktopMemo.Services.Mapping;

namespace DesktopMemo.Services.Services;

public class NoteService : INoteService
{
    private readonly IDbContextFactory _contextFactory;

    public NoteService(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public NoteDto CreateNote(CreateNoteRequest req)
    {
        if (req == null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        using (var context = _contextFactory.Create())
        {
            var group = context.MemoGroups.FirstOrDefault(x => x.Id == req.GroupId && !x.IsDeleted);
            if (group == null)
            {
                throw new InvalidOperationException("메모 그룹을 찾을 수 없습니다.");
            }

            var now = DateTime.UtcNow;
            var note = new Note
            {
                Id = Guid.NewGuid(),
                GroupId = req.GroupId,
                Title = string.IsNullOrWhiteSpace(req.Title) ? "새 메모" : req.Title.Trim(),
                ContentMarkdown = req.ContentMarkdown ?? string.Empty,
                FontFamily = req.FontFamily ?? "Segoe UI",
                FontSize = req.FontSize <= 0 ? 14 : req.FontSize,
                FontWeight = req.FontWeight ?? "Normal",
                FontStyle = req.FontStyle ?? "Normal",
                IsUnderline = req.IsUnderline,
                FontColorHex = req.FontColorHex ?? "#000000",
                AlarmEnabled = req.AlarmEnabled,
                AlarmAtUtc = req.AlarmAtUtc,
                TimeZoneId = string.IsNullOrWhiteSpace(req.TimeZoneId) ? TimeZoneInfo.Local.Id : req.TimeZoneId,
                RepeatType = req.RepeatType,
                RepeatEndUtc = req.RepeatEndUtc,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Notes.Add(note);
            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.ToString().Contains("FOREIGN KEY constraint failed"))
            {
                // Defensive recovery for legacy DB rows with broken group references.
                var fallbackGroup = context.MemoGroups.FirstOrDefault(x => !x.IsDeleted && x.Name == "Inbox");
                if (fallbackGroup == null)
                {
                    throw;
                }

                note.GroupId = fallbackGroup.Id;
                context.SaveChanges();
            }

            return EntityMapper.ToDto(note);
        }
    }

    public NoteDto UpdateNote(UpdateNoteRequest req)
    {
        if (req == null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == req.Id && !x.IsDeleted);
            if (note == null)
            {
                throw new InvalidOperationException("메모를 찾을 수 없습니다.");
            }

            note.Title = string.IsNullOrWhiteSpace(req.Title) ? "새 메모" : req.Title.Trim();
            note.ContentMarkdown = req.ContentMarkdown ?? string.Empty;
            note.FontFamily = string.IsNullOrWhiteSpace(req.FontFamily) ? "Segoe UI" : req.FontFamily;
            note.FontSize = req.FontSize <= 0 ? 14 : req.FontSize;
            note.FontWeight = string.IsNullOrWhiteSpace(req.FontWeight) ? "Normal" : req.FontWeight;
            note.FontStyle = string.IsNullOrWhiteSpace(req.FontStyle) ? "Normal" : req.FontStyle;
            note.IsUnderline = req.IsUnderline;
            note.FontColorHex = string.IsNullOrWhiteSpace(req.FontColorHex) ? "#000000" : req.FontColorHex;
            note.AlarmEnabled = req.AlarmEnabled;
            note.AlarmAtUtc = req.AlarmAtUtc;
            note.TimeZoneId = string.IsNullOrWhiteSpace(req.TimeZoneId) ? TimeZoneInfo.Local.Id : req.TimeZoneId;
            note.RepeatType = req.RepeatType;
            note.RepeatEndUtc = req.RepeatEndUtc;
            note.UpdatedAtUtc = DateTime.UtcNow;

            if (!note.AlarmEnabled)
            {
                note.SnoozeUntilUtc = null;
            }

            context.SaveChanges();
            return EntityMapper.ToDto(note);
        }
    }

    public void MoveNote(Guid noteId, Guid targetGroupId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                throw new InvalidOperationException("메모를 찾을 수 없습니다.");
            }

            var target = context.MemoGroups.FirstOrDefault(x => x.Id == targetGroupId && !x.IsDeleted);
            if (target == null)
            {
                throw new InvalidOperationException("대상 그룹을 찾을 수 없습니다.");
            }

            note.GroupId = targetGroupId;
            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public void SoftDeleteNote(Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                return;
            }

            note.IsDeleted = true;
            note.DeletedAtUtc = DateTime.UtcNow;
            note.UpdatedAtUtc = DateTime.UtcNow;
            context.SaveChanges();
        }
    }

    public IList<NoteDto> GetNotesByGroup(Guid groupId)
    {
        using (var context = _contextFactory.Create())
        {
            return context.Notes
                .Where(x => x.GroupId == groupId && !x.IsDeleted)
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ToList()
                .Select(EntityMapper.ToDto)
                .ToList();
        }
    }

    public NoteDto GetNote(Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var note = context.Notes.FirstOrDefault(x => x.Id == noteId && !x.IsDeleted);
            if (note == null)
            {
                return null;
            }

            return EntityMapper.ToDto(note);
        }
    }
}
