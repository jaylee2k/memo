using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.Domain;

[TestClass]
public class DefaultValueTests
{
    [TestMethod]
    public void Note_Ctor_SetsExpectedDefaults()
    {
        var note = new Note();

        Assert.AreEqual("Segoe UI", note.FontFamily);
        Assert.AreEqual("Normal", note.FontWeight);
        Assert.AreEqual("Normal", note.FontStyle);
        Assert.AreEqual("#000000", note.FontColorHex);
        Assert.AreEqual(14d, note.FontSize);
    }

    [TestMethod]
    public void FontSettingDto_Ctor_SetsExpectedDefaults()
    {
        var dto = new FontSettingDto();

        Assert.AreEqual("Segoe UI", dto.FontFamily);
        Assert.AreEqual(14d, dto.FontSize);
        Assert.AreEqual("Normal", dto.FontWeight);
        Assert.AreEqual("Normal", dto.FontStyle);
        Assert.AreEqual("#000000", dto.FontColorHex);
        Assert.IsFalse(dto.IsUnderline);
    }
}
