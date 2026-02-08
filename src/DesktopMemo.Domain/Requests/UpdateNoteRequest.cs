using System;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.Domain.Requests;

public class UpdateNoteRequest
{
    public Guid Id { get; set; }
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

    public UpdateNoteRequest()
    {
        Title = string.Empty;
        ContentMarkdown = string.Empty;
        FontFamily = "Segoe UI";
        FontSize = 14;
        FontWeight = "Normal";
        FontStyle = "Normal";
        FontColorHex = "#000000";
        TimeZoneId = "Korea Standard Time";
    }
}
