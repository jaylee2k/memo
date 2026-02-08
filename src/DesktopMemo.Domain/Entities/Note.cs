using System;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.Domain.Entities;

public class Note
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; }
    public string ContentMarkdown { get; set; }

    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public string FontWeight { get; set; }
    public string FontStyle { get; set; }
    public bool IsUnderline { get; set; }
    public string FontColorHex { get; set; }

    public bool AlarmEnabled { get; set; }
    public DateTime? AlarmAtUtc { get; set; }
    public string TimeZoneId { get; set; }
    public RepeatType RepeatType { get; set; }
    public DateTime? RepeatEndUtc { get; set; }
    public DateTime? SnoozeUntilUtc { get; set; }
    public DateTime? LastTriggeredAtUtc { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public virtual MemoGroup Group { get; set; }
    public virtual StickyWindowState StickyWindowState { get; set; }

    public Note()
    {
        Title = string.Empty;
        ContentMarkdown = string.Empty;
        FontFamily = "Segoe UI";
        FontWeight = "Normal";
        FontStyle = "Normal";
        FontColorHex = "#000000";
        FontSize = 14;
        TimeZoneId = "Korea Standard Time";
    }
}
