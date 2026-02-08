using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DesktopMemo.Domain.Entities;

namespace DesktopMemo.Data.Persistence;

public static class DatabaseBootstrapper
{
    private static readonly IReadOnlyList<string> SchemaStatements = new List<string>
    {
        @"CREATE TABLE IF NOT EXISTS [MemoGroups] (
            [Id] TEXT NOT NULL PRIMARY KEY,
            [ParentGroupId] TEXT NULL,
            [Name] TEXT NOT NULL,
            [SortOrder] INTEGER NOT NULL,
            [IsDeleted] INTEGER NOT NULL,
            [DeletedAtUtc] TEXT NULL,
            [CreatedAtUtc] TEXT NOT NULL,
            [UpdatedAtUtc] TEXT NOT NULL,
            FOREIGN KEY([ParentGroupId]) REFERENCES [MemoGroups]([Id])
        );",
        @"CREATE TABLE IF NOT EXISTS [Notes] (
            [Id] TEXT NOT NULL PRIMARY KEY,
            [GroupId] TEXT NOT NULL,
            [Title] TEXT NOT NULL,
            [ContentMarkdown] TEXT NOT NULL,
            [FontFamily] TEXT NOT NULL,
            [FontSize] REAL NOT NULL,
            [FontWeight] TEXT NOT NULL,
            [FontStyle] TEXT NOT NULL,
            [IsUnderline] INTEGER NOT NULL,
            [FontColorHex] TEXT NOT NULL,
            [AlarmEnabled] INTEGER NOT NULL,
            [AlarmAtUtc] TEXT NULL,
            [TimeZoneId] TEXT NOT NULL,
            [RepeatType] INTEGER NOT NULL,
            [RepeatEndUtc] TEXT NULL,
            [SnoozeUntilUtc] TEXT NULL,
            [LastTriggeredAtUtc] TEXT NULL,
            [IsDeleted] INTEGER NOT NULL,
            [DeletedAtUtc] TEXT NULL,
            [CreatedAtUtc] TEXT NOT NULL,
            [UpdatedAtUtc] TEXT NOT NULL,
            FOREIGN KEY([GroupId]) REFERENCES [MemoGroups]([Id])
        );",
        @"CREATE TABLE IF NOT EXISTS [StickyWindowStates] (
            [NoteId] TEXT NOT NULL PRIMARY KEY,
            [Left] REAL NOT NULL,
            [Top] REAL NOT NULL,
            [Width] REAL NOT NULL,
            [Height] REAL NOT NULL,
            [IsAlwaysOnTop] INTEGER NOT NULL,
            [LastOpenedAtUtc] TEXT NOT NULL,
            FOREIGN KEY([NoteId]) REFERENCES [Notes]([Id])
        );",
        @"CREATE TABLE IF NOT EXISTS [AppSettings] (
            [Key] TEXT NOT NULL PRIMARY KEY,
            [Value] TEXT NOT NULL,
            [UpdatedAtUtc] TEXT NOT NULL
        );",
        "CREATE INDEX IF NOT EXISTS [IX_MemoGroups_ParentGroup_IsDeleted] ON [MemoGroups]([ParentGroupId], [IsDeleted]);",
        "CREATE INDEX IF NOT EXISTS [IX_MemoGroups_DeletedAt] ON [MemoGroups]([DeletedAtUtc]);",
        "CREATE INDEX IF NOT EXISTS [IX_Notes_Group_IsDeleted] ON [Notes]([GroupId], [IsDeleted]);",
        "CREATE INDEX IF NOT EXISTS [IX_Notes_Alarm] ON [Notes]([AlarmEnabled], [AlarmAtUtc], [SnoozeUntilUtc], [IsDeleted]);",
        "CREATE INDEX IF NOT EXISTS [IX_Notes_DeletedAt] ON [Notes]([DeletedAtUtc]);"
    };

    public static void Initialize()
    {
        using (var context = new DesktopMemoDbContext())
        {
            foreach (var sql in SchemaStatements)
            {
                context.Database.ExecuteSqlCommand(sql);
            }

            NormalizeLegacyGuidStorage(context);
            DesktopMemoDatabaseSeeder.EnsureSeedData(context);
        }
    }

    private static void NormalizeLegacyGuidStorage(DesktopMemoDbContext context)
    {
        var blobCount = context.Database.SqlQuery<long>(
            "SELECT " +
            "(SELECT COUNT(1) FROM [MemoGroups] WHERE typeof([Id])='blob' OR typeof([ParentGroupId])='blob') + " +
            "(SELECT COUNT(1) FROM [Notes] WHERE typeof([Id])='blob' OR typeof([GroupId])='blob') + " +
            "(SELECT COUNT(1) FROM [StickyWindowStates] WHERE typeof([NoteId])='blob');")
            .FirstOrDefault();

        if (blobCount <= 0)
        {
            return;
        }

        var groups = context.MemoGroups.AsNoTracking().ToList();
        var notes = context.Notes.AsNoTracking().ToList();
        var states = context.StickyWindowStates.AsNoTracking().ToList();

        context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = OFF;");
        context.Database.ExecuteSqlCommand("DELETE FROM [StickyWindowStates];");
        context.Database.ExecuteSqlCommand("DELETE FROM [Notes];");
        context.Database.ExecuteSqlCommand("DELETE FROM [MemoGroups];");

        foreach (var group in groups)
        {
            context.MemoGroups.Add(new MemoGroup
            {
                Id = group.Id,
                ParentGroupId = group.ParentGroupId,
                Name = group.Name,
                SortOrder = group.SortOrder,
                IsDeleted = group.IsDeleted,
                DeletedAtUtc = group.DeletedAtUtc,
                CreatedAtUtc = group.CreatedAtUtc,
                UpdatedAtUtc = group.UpdatedAtUtc
            });
        }

        foreach (var note in notes)
        {
            context.Notes.Add(new Note
            {
                Id = note.Id,
                GroupId = note.GroupId,
                Title = note.Title,
                ContentMarkdown = note.ContentMarkdown,
                FontFamily = note.FontFamily,
                FontSize = note.FontSize,
                FontWeight = note.FontWeight,
                FontStyle = note.FontStyle,
                IsUnderline = note.IsUnderline,
                FontColorHex = note.FontColorHex,
                AlarmEnabled = note.AlarmEnabled,
                AlarmAtUtc = note.AlarmAtUtc,
                TimeZoneId = note.TimeZoneId,
                RepeatType = note.RepeatType,
                RepeatEndUtc = note.RepeatEndUtc,
                SnoozeUntilUtc = note.SnoozeUntilUtc,
                LastTriggeredAtUtc = note.LastTriggeredAtUtc,
                IsDeleted = note.IsDeleted,
                DeletedAtUtc = note.DeletedAtUtc,
                CreatedAtUtc = note.CreatedAtUtc,
                UpdatedAtUtc = note.UpdatedAtUtc
            });
        }

        foreach (var state in states)
        {
            context.StickyWindowStates.Add(new StickyWindowState
            {
                NoteId = state.NoteId,
                Left = state.Left,
                Top = state.Top,
                Width = state.Width,
                Height = state.Height,
                IsAlwaysOnTop = state.IsAlwaysOnTop,
                LastOpenedAtUtc = state.LastOpenedAtUtc
            });
        }

        context.SaveChanges();
        context.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON;");
    }
}
