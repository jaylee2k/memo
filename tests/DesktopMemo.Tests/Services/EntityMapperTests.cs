using System;
using System.Collections.Generic;
using System.Linq;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Services.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.Services;

[TestClass]
public class EntityMapperTests
{
    [TestMethod]
    public void ToFontSetting_UsesDefaults_WhenValuesInvalid()
    {
        var result = EntityMapper.ToFontSetting("", "not-number", "", "", "invalid", "");

        Assert.AreEqual("Segoe UI", result.FontFamily);
        Assert.AreEqual(14d, result.FontSize);
        Assert.AreEqual("Normal", result.FontWeight);
        Assert.AreEqual("Normal", result.FontStyle);
        Assert.IsFalse(result.IsUnderline);
        Assert.AreEqual("#000000", result.FontColorHex);
    }

    [TestMethod]
    public void ToFontSetting_MapsValues_WhenInputsValid()
    {
        var result = EntityMapper.ToFontSetting("Consolas", "16.5", "Bold", "Italic", "true", "#FFAA00");

        Assert.AreEqual("Consolas", result.FontFamily);
        Assert.AreEqual(16.5d, result.FontSize);
        Assert.AreEqual("Bold", result.FontWeight);
        Assert.AreEqual("Italic", result.FontStyle);
        Assert.IsTrue(result.IsUnderline);
        Assert.AreEqual("#FFAA00", result.FontColorHex);
    }

    [TestMethod]
    public void CollectDescendantGroupIds_ReturnsRootAndDescendants_WithoutDuplicates()
    {
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandChildId = Guid.NewGuid();
        var unrelatedId = Guid.NewGuid();

        var groups = new List<MemoGroup>
        {
            new MemoGroup { Id = rootId, Name = "Root" },
            new MemoGroup { Id = childId, ParentGroupId = rootId, Name = "Child" },
            new MemoGroup { Id = grandChildId, ParentGroupId = childId, Name = "Grand" },
            new MemoGroup { Id = unrelatedId, Name = "Other" },
            new MemoGroup { Id = childId, ParentGroupId = rootId, Name = "ChildDuplicate" }
        };

        var result = EntityMapper.CollectDescendantGroupIds(groups.AsQueryable(), rootId);

        CollectionAssert.AreEquivalent(new[] { rootId, childId, grandChildId }, result);
        Assert.IsFalse(result.Contains(unrelatedId));
    }
}
