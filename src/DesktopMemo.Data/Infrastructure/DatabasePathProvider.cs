using System;
using System.IO;

namespace DesktopMemo.Data.Infrastructure;

public static class DatabasePathProvider
{
    public const string DatabasePathEnvironmentVariable = "DESKTOPMEMO_DB_PATH";

    public static string GetDatabasePath()
    {
        var overriddenPath = Environment.GetEnvironmentVariable(DatabasePathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overriddenPath))
        {
            var overrideDirectory = Path.GetDirectoryName(overriddenPath);
            if (!string.IsNullOrWhiteSpace(overrideDirectory))
            {
                Directory.CreateDirectory(overrideDirectory);
            }

            return overriddenPath;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(appData, "DesktopMemo");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "desktopmemo.db");
    }
}
