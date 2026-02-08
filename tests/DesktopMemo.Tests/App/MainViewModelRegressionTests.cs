using System;
using System.Collections.Generic;
using DesktopMemo.App.Services;
using DesktopMemo.App.ViewModels;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Domain.Requests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopMemo.Tests.App;

[TestClass]
public class MainViewModelRegressionTests
{
    [TestMethod]
    public void AddNoteCommand_WhenNoteServiceThrows_DoesNotPropagateAndSetsFailureStatus()
    {
        var dialog = new FakeDialogService();
        var viewModel = new MainViewModel(
            new FakeGroupService(),
            new ThrowingNoteService(),
            new FakeTrashService(),
            new FakeStickyNoteService(),
            new FakeSettingsService(),
            new FakeAlarmService(),
            dialog);

        viewModel.AddNoteCommand.Execute(null);

        Assert.AreEqual("메모 생성에 실패했습니다.", viewModel.StatusMessage);
        Assert.AreEqual(1, dialog.ErrorCount);
    }

    [TestMethod]
    public void Validation_WhenAlarmEnabledAndAlarmTimeInvalid_HasValidationError()
    {
        var viewModel = new MainViewModel(
            new FakeGroupService(),
            new ThrowingNoteService(),
            new FakeTrashService(),
            new FakeStickyNoteService(),
            new FakeSettingsService(),
            new FakeAlarmService(),
            new FakeDialogService());

        viewModel.AlarmEnabled = true;
        viewModel.AlarmTimeText = "25:00";

        Assert.IsTrue(viewModel.HasValidationErrors);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel[nameof(MainViewModel.AlarmTimeText)]));
    }

    [TestMethod]
    public void Validation_WhenFontColorHexInvalid_HasValidationError()
    {
        var viewModel = new MainViewModel(
            new FakeGroupService(),
            new ThrowingNoteService(),
            new FakeTrashService(),
            new FakeStickyNoteService(),
            new FakeSettingsService(),
            new FakeAlarmService(),
            new FakeDialogService());

        viewModel.FontColorHex = "red";

        Assert.IsTrue(viewModel.HasValidationErrors);
        Assert.IsFalse(string.IsNullOrEmpty(viewModel[nameof(MainViewModel.FontColorHex)]));
    }

    private sealed class FakeDialogService : IUserDialogService
    {
        public int ErrorCount { get; private set; }

        public string Prompt(string title, string message, string initialValue = "") => initialValue;
        public bool Confirm(string title, string message) => true;
        public void Error(string title, string message) => ErrorCount++;
        public void Info(string title, string message) { }
    }

    private sealed class FakeGroupService : IGroupService
    {
        private readonly Guid _inboxId = Guid.NewGuid();

        public GroupTreeNodeDto CreateGroup(CreateGroupRequest req) => throw new NotImplementedException();
        public GroupTreeNodeDto UpdateGroup(UpdateGroupRequest req) => throw new NotImplementedException();
        public void SoftDeleteGroup(Guid groupId) => throw new NotImplementedException();
        public IList<GroupTreeNodeDto> GetGroupTree() => new List<GroupTreeNodeDto>();
        public Guid GetOrCreateInboxGroupId() => _inboxId;
    }

    private sealed class ThrowingNoteService : INoteService
    {
        public NoteDto CreateNote(CreateNoteRequest req) => throw new InvalidOperationException("forced");
        public NoteDto UpdateNote(UpdateNoteRequest req) => throw new NotImplementedException();
        public void MoveNote(Guid noteId, Guid targetGroupId) => throw new NotImplementedException();
        public void SoftDeleteNote(Guid noteId) => throw new NotImplementedException();
        public IList<NoteDto> GetNotesByGroup(Guid groupId) => new List<NoteDto>();
        public NoteDto GetNote(Guid noteId) => null;
    }

    private sealed class FakeTrashService : ITrashService
    {
        public IList<TrashItemDto> GetTrashItems() => new List<TrashItemDto>();
        public void RestoreNote(Guid noteId) { }
        public void RestoreGroup(Guid groupId) { }
        public void DeleteItemPermanently(Guid itemId, TrashItemType itemType) { }
        public void PurgeExpiredItems() { }
        public void EmptyTrash() { }
    }

    private sealed class FakeStickyNoteService : IStickyNoteService
    {
        public void OpenSticky(Guid noteId) { }
        public void CloseSticky(Guid noteId) { }
        public void SaveWindowState(Guid noteId, StickyWindowStateDto state) { }
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public FontSettingDto GetGlobalFontSetting() => new FontSettingDto();
        public void UpdateGlobalFontSetting(FontSettingDto dto) { }
    }

    private sealed class FakeAlarmService : IAlarmService
    {
        public void ScheduleOrUpdate(Guid noteId) { }
        public void Dismiss(Guid noteId) { }
        public void Snooze(Guid noteId, int minutes) { }
        public void ProcessDueAlarms() { }
    }
}
