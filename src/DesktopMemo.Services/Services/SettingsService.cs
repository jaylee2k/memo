using System;
using System.Linq;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Services.Mapping;

namespace DesktopMemo.Services.Services;

public class SettingsService : ISettingsService
{
    private readonly IDbContextFactory _contextFactory;

    public SettingsService(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public FontSettingDto GetGlobalFontSetting()
    {
        using (var context = _contextFactory.Create())
        {
            var settings = context.AppSettings.ToList();
            string Get(string key, string fallback)
            {
                var row = settings.FirstOrDefault(x => x.Key == key);
                return row == null ? fallback : row.Value;
            }

            return EntityMapper.ToFontSetting(
                Get("GlobalFontFamily", "Segoe UI"),
                Get("GlobalFontSize", "14"),
                Get("GlobalFontWeight", "Normal"),
                Get("GlobalFontStyle", "Normal"),
                Get("GlobalUnderline", "false"),
                Get("GlobalFontColorHex", "#000000"));
        }
    }

    public void UpdateGlobalFontSetting(FontSettingDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        using (var context = _contextFactory.Create())
        {
            Upsert(context, "GlobalFontFamily", dto.FontFamily ?? "Segoe UI");
            Upsert(context, "GlobalFontSize", dto.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Upsert(context, "GlobalFontWeight", dto.FontWeight ?? "Normal");
            Upsert(context, "GlobalFontStyle", dto.FontStyle ?? "Normal");
            Upsert(context, "GlobalUnderline", dto.IsUnderline.ToString());
            Upsert(context, "GlobalFontColorHex", dto.FontColorHex ?? "#000000");

            context.SaveChanges();
        }
    }

    private static void Upsert(DesktopMemo.Data.Persistence.DesktopMemoDbContext context, string key, string value)
    {
        var row = context.AppSettings.FirstOrDefault(x => x.Key == key);
        if (row == null)
        {
            row = new AppSetting
            {
                Key = key,
                Value = value,
                UpdatedAtUtc = DateTime.UtcNow
            };
            context.AppSettings.Add(row);
            return;
        }

        row.Value = value;
        row.UpdatedAtUtc = DateTime.UtcNow;
    }
}
