using System;
using System.IO;
using System.Linq;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Data.Persistence;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Domain.Requests;
using DesktopMemo.Services.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.Data;

[TestClass]
public class DatabaseBootstrapperRegressionTests
{
    private string _dbPath;
    private bool _sqliteAvailable;
    private string _sqliteUnavailableReason;

    [TestInitialize]
    public void SetUp()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), "DesktopMemo.Tests", $"db-{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable(DatabasePathProvider.DatabasePathEnvironmentVariable, _dbPath);

        _sqliteAvailable = IsSqliteAvailable(out var reason);
        _sqliteUnavailableReason = reason;
    }

    [TestCleanup]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(DatabasePathProvider.DatabasePathEnvironmentVariable, null);

        TryDelete(_dbPath);
        TryDelete(_dbPath + "-wal");
        TryDelete(_dbPath + "-shm");
    }

    [TestMethod]
    public void Initialize_CanRunTwice_AndEnsuresRequiredSchemaAndSeed()
    {
        EnsureSqliteAvailable();

        DatabaseBootstrapper.Initialize();
        DatabaseBootstrapper.Initialize();

        using (var context = new DesktopMemoDbContext())
        {
            var tables = context.Database
                .SqlQuery<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
                .ToList();

            CollectionAssert.IsSubsetOf(
                new[] { "MemoGroups", "Notes", "StickyWindowStates", "AppSettings" },
                tables);

            var inboxCount = context.MemoGroups.Count(x => x.Name == "Inbox" && !x.IsDeleted);
            Assert.IsTrue(inboxCount >= 1, "Inbox seed row must exist.");
        }
    }

    [TestMethod]
    public void GuidColumns_AreStoredAsText_AndNoteInsertWithForeignKeySucceeds()
    {
        EnsureSqliteAvailable();

        DatabaseBootstrapper.Initialize();

        var contextFactory = new DbContextFactory();
        var groupService = new GroupService(contextFactory);
        var noteService = new NoteService(contextFactory);

        var inboxId = groupService.GetOrCreateInboxGroupId();

        var created = noteService.CreateNote(new CreateNoteRequest
        {
            GroupId = inboxId,
            Title = "FK Regression Test",
            ContentMarkdown = "body",
            AlarmEnabled = false,
            RepeatType = RepeatType.None,
            TimeZoneId = "Korea Standard Time"
        });

        using (var context = contextFactory.Create())
        {
            var memoGroupIdType = context.Database.SqlQuery<string>(
                "SELECT typeof([Id]) FROM [MemoGroups] WHERE [Id] = {0};",
                inboxId.ToString())
                .FirstOrDefault();

            var noteGroupIdType = context.Database.SqlQuery<string>(
                "SELECT typeof([GroupId]) FROM [Notes] WHERE [Id] = {0};",
                created.Id.ToString())
                .FirstOrDefault();

            Assert.AreEqual("text", memoGroupIdType);
            Assert.AreEqual("text", noteGroupIdType);

            var fkJoinCount = context.Database.SqlQuery<long>(
                "SELECT COUNT(1) FROM [Notes] n INNER JOIN [MemoGroups] g ON n.[GroupId] = g.[Id] WHERE n.[Id] = {0};",
                created.Id.ToString())
                .FirstOrDefault();

            Assert.AreEqual(1L, fkJoinCount, "Inserted note must satisfy GroupId FK relation.");
        }
    }

    private void EnsureSqliteAvailable()
    {
        if (_sqliteAvailable)
        {
            return;
        }

        Assert.Inconclusive("SQLite native runtime is unavailable in current test host: " + _sqliteUnavailableReason);
    }

    private static bool IsSqliteAvailable(out string reason)
    {
        try
        {
            using (var context = new DesktopMemoDbContext())
            {
                context.Database.Connection.Open();
                context.Database.Connection.Close();
            }

            reason = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            reason = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests.
        }
    }
}
