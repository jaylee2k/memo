using System.IO;
using DesktopMemo.Data.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.Data;

[TestClass]
public class DatabasePathProviderTests
{
    [TestMethod]
    public void GetDatabasePath_ReturnsDesktopMemoDbPath_AndEnsuresDirectory()
    {
        var path = DatabasePathProvider.GetDatabasePath();
        var directory = Path.GetDirectoryName(path);

        StringAssert.EndsWith(path.Replace('/', '\\'), "DesktopMemo\\desktopmemo.db");
        Assert.IsFalse(string.IsNullOrWhiteSpace(directory));
        Assert.IsTrue(Directory.Exists(directory));
    }
}
