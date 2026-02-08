using DesktopMemo.App.Services;
using DesktopMemo.App.ViewModels;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.App;

[TestClass]
public class SettingsViewModelValidationTests
{
    [TestMethod]
    public void SaveCommand_WhenInvalidInput_DoesNotPersistAndShowsError()
    {
        var settingsService = new FakeSettingsService();
        var dialogService = new FakeDialogService();
        var viewModel = new SettingsViewModel(settingsService, dialogService)
        {
            FontColorHex = "red"
        };

        viewModel.SaveCommand.Execute(null);

        Assert.AreEqual(0, settingsService.UpdateCallCount);
        Assert.AreEqual(1, dialogService.ErrorCount);
    }

    [TestMethod]
    public void SaveCommand_WhenValidInput_PersistsAndShowsInfo()
    {
        var settingsService = new FakeSettingsService();
        var dialogService = new FakeDialogService();
        var viewModel = new SettingsViewModel(settingsService, dialogService)
        {
            FontSize = 16,
            FontColorHex = "#112233"
        };

        viewModel.SaveCommand.Execute(null);

        Assert.AreEqual(1, settingsService.UpdateCallCount);
        Assert.AreEqual(1, dialogService.InfoCount);
        Assert.AreEqual(0, dialogService.ErrorCount);
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public int UpdateCallCount { get; private set; }

        public FontSettingDto GetGlobalFontSetting()
        {
            return new FontSettingDto();
        }

        public void UpdateGlobalFontSetting(FontSettingDto dto)
        {
            UpdateCallCount++;
        }
    }

    private sealed class FakeDialogService : IUserDialogService
    {
        public int ErrorCount { get; private set; }
        public int InfoCount { get; private set; }

        public string Prompt(string title, string message, string initialValue = "")
        {
            return initialValue;
        }

        public bool Confirm(string title, string message)
        {
            return true;
        }

        public void Error(string title, string message)
        {
            ErrorCount++;
        }

        public void Info(string title, string message)
        {
            InfoCount++;
        }
    }
}
