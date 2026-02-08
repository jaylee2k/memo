using System;
using System.Linq;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;

namespace DesktopMemo.Services.Mapping;

internal static class EntityMapper
{
    public static NoteDto ToDto(Note entity)
    {
        return new NoteDto
        {
            Id = entity.Id,
            GroupId = entity.GroupId,
            Title = entity.Title,
            ContentMarkdown = entity.ContentMarkdown,
            FontFamily = entity.FontFamily,
            FontSize = entity.FontSize,
            FontWeight = entity.FontWeight,
            FontStyle = entity.FontStyle,
            IsUnderline = entity.IsUnderline,
            FontColorHex = entity.FontColorHex,
            AlarmEnabled = entity.AlarmEnabled,
            AlarmAtUtc = entity.AlarmAtUtc,
            TimeZoneId = entity.TimeZoneId,
            RepeatType = entity.RepeatType,
            RepeatEndUtc = entity.RepeatEndUtc,
            SnoozeUntilUtc = entity.SnoozeUntilUtc,
            LastTriggeredAtUtc = entity.LastTriggeredAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    public static GroupTreeNodeDto ToNode(MemoGroup group)
    {
        return new GroupTreeNodeDto
        {
            Id = group.Id,
            ParentGroupId = group.ParentGroupId,
            Name = group.Name,
            SortOrder = group.SortOrder
        };
    }

    public static FontSettingDto ToFontSetting(string family, string size, string weight, string style, string underline, string color)
    {
        double parsedSize;
        if (!double.TryParse(size, out parsedSize))
        {
            parsedSize = 14;
        }

        bool parsedUnderline;
        if (!bool.TryParse(underline, out parsedUnderline))
        {
            parsedUnderline = false;
        }

        return new FontSettingDto
        {
            FontFamily = string.IsNullOrWhiteSpace(family) ? "Segoe UI" : family,
            FontSize = parsedSize,
            FontWeight = string.IsNullOrWhiteSpace(weight) ? "Normal" : weight,
            FontStyle = string.IsNullOrWhiteSpace(style) ? "Normal" : style,
            IsUnderline = parsedUnderline,
            FontColorHex = string.IsNullOrWhiteSpace(color) ? "#000000" : color
        };
    }

    public static Guid[] CollectDescendantGroupIds(IQueryable<MemoGroup> groups, Guid rootId)
    {
        var all = groups.ToList();
        var collected = all.Where(x => x.Id == rootId).Select(x => x.Id).ToList();
        var queue = new System.Collections.Generic.Queue<Guid>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = all.Where(x => x.ParentGroupId == current).Select(x => x.Id).ToList();
            foreach (var child in children)
            {
                if (collected.Contains(child))
                {
                    continue;
                }

                collected.Add(child);
                queue.Enqueue(child);
            }
        }

        return collected.ToArray();
    }
}
