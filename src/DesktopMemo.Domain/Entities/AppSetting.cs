using System;

namespace DesktopMemo.Domain.Entities;

public class AppSetting
{
    public string Key { get; set; }
    public string Value { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public AppSetting()
    {
        Key = string.Empty;
        Value = string.Empty;
    }
}
