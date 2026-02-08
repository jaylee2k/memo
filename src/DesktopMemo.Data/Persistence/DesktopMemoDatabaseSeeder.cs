using System;

namespace DesktopMemo.Data.Persistence;

internal static class DesktopMemoDatabaseSeeder
{
    public static void EnsureSeedData(DesktopMemoDbContext context)
    {
        var now = DateTime.UtcNow;
        var nowText = now.ToString("o");
        var inboxId = Guid.NewGuid().ToString();

        context.Database.ExecuteSqlCommand(
            "INSERT INTO [MemoGroups] ([Id],[ParentGroupId],[Name],[SortOrder],[IsDeleted],[DeletedAtUtc],[CreatedAtUtc],[UpdatedAtUtc]) " +
            "SELECT {0}, NULL, 'Inbox', 0, 0, NULL, {1}, {1} " +
            "WHERE NOT EXISTS (SELECT 1 FROM [MemoGroups] WHERE [Name] = 'Inbox' AND [IsDeleted] = 0);",
            inboxId,
            nowText);

        InsertSettingIfMissing(context, "GlobalFontFamily", "Segoe UI", nowText);
        InsertSettingIfMissing(context, "GlobalFontSize", "14", nowText);
        InsertSettingIfMissing(context, "GlobalFontWeight", "Normal", nowText);
        InsertSettingIfMissing(context, "GlobalFontStyle", "Normal", nowText);
        InsertSettingIfMissing(context, "GlobalUnderline", "false", nowText);
        InsertSettingIfMissing(context, "GlobalFontColorHex", "#000000", nowText);
    }

    private static void InsertSettingIfMissing(DesktopMemoDbContext context, string key, string value, string nowText)
    {
        context.Database.ExecuteSqlCommand(
            "INSERT OR IGNORE INTO [AppSettings] ([Key], [Value], [UpdatedAtUtc]) VALUES ({0}, {1}, {2});",
            key,
            value,
            nowText);
    }
}
