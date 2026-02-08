using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopMemo.App.Infrastructure;
using DesktopMemo.App.Services;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Enums;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Domain.Requests;
using Markdig;

namespace DesktopMemo.App.ViewModels;

public class MainViewModel : ObservableObject, IDataErrorInfo
{
    private static readonly Regex HexColorRegex = new Regex("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$");

    private readonly IGroupService _groupService;
    private readonly INoteService _noteService;
    private readonly ITrashService _trashService;
    private readonly IStickyNoteService _stickyNoteService;
    private readonly ISettingsService _settingsService;
    private readonly IAlarmService _alarmService;
    private readonly IUserDialogService _dialogService;
    private readonly Debouncer _autoSaveDebouncer;

    private bool _initialized;
    private bool _suspendAutoSave;

    private GroupNodeViewModel _selectedGroup;
    private NoteListItemViewModel _selectedNote;
    private GroupNodeViewModel _moveTargetGroup;
    private TrashItemViewModel _selectedTrashItem;

    private string _noteTitle;
    private string _noteContent;
    private string _selectedFontFamily;
    private string _selectedFontWeight;
    private string _selectedFontStyle;
    private string _fontColorHex;
    private double _fontSize;
    private bool _isUnderline;

    private bool _alarmEnabled;
    private DateTime? _alarmLocalDate;
    private string _alarmTimeText;
    private RepeatType _selectedRepeatType;
    private DateTime? _repeatEndLocalDate;

    private string _statusMessage;
    private string _markdownPreviewHtml;
    private string _searchKeyword;

    public ObservableCollection<GroupNodeViewModel> GroupTree { get; }
    public ObservableCollection<GroupNodeViewModel> AllGroupsFlat { get; }
    public ObservableCollection<NoteListItemViewModel> Notes { get; }
    public ObservableCollection<TrashItemViewModel> TrashItems { get; }
    public ICollectionView FilteredNotesView { get; }

    public IReadOnlyList<string> FontFamilies { get; }
    public IReadOnlyList<string> FontWeights { get; }
    public IReadOnlyList<string> FontStyles { get; }
    public IReadOnlyList<RepeatType> RepeatTypes { get; }

    public IRelayCommand InitializeCommand { get; }
    public IRelayCommand AddGroupCommand { get; }
    public IRelayCommand RenameGroupCommand { get; }
    public IRelayCommand DeleteGroupCommand { get; }
    public IRelayCommand AddNoteCommand { get; }
    public IRelayCommand DeleteNoteCommand { get; }
    public IRelayCommand MoveNoteCommand { get; }
    public IRelayCommand OpenStickyCommand { get; }
    public IRelayCommand RestoreTrashItemCommand { get; }
    public IRelayCommand EmptyTrashCommand { get; }
    public IRelayCommand PurgeExpiredTrashCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand Snooze5Command { get; }
    public IRelayCommand Snooze10Command { get; }
    public IRelayCommand Snooze30Command { get; }
    public IRelayCommand DismissAlarmCommand { get; }
    public IRelayCommand DeleteTrashItemPermanentlyCommand { get; }

    public GroupNodeViewModel SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                LoadNotesForSelectedGroup();
                MoveTargetGroup = value;
                OnPropertyChanged(nameof(HasSelectedGroup));
            }
        }
    }

    public NoteListItemViewModel SelectedNote
    {
        get => _selectedNote;
        set
        {
            if (SetProperty(ref _selectedNote, value))
            {
                LoadEditorFromSelectedNote();
                OnPropertyChanged(nameof(HasSelectedNote));
            }
        }
    }

    public GroupNodeViewModel MoveTargetGroup
    {
        get => _moveTargetGroup;
        set => SetProperty(ref _moveTargetGroup, value);
    }

    public TrashItemViewModel SelectedTrashItem
    {
        get => _selectedTrashItem;
        set => SetProperty(ref _selectedTrashItem, value);
    }

    public string NoteTitle
    {
        get => _noteTitle;
        set
        {
            if (SetProperty(ref _noteTitle, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string NoteContent
    {
        get => _noteContent;
        set
        {
            if (SetProperty(ref _noteContent, value))
            {
                MarkdownPreviewHtml = BuildMarkdownHtml(value);
                QueueAutoSave();
            }
        }
    }

    public string SelectedFontFamily
    {
        get => _selectedFontFamily;
        set
        {
            if (SetProperty(ref _selectedFontFamily, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string SelectedFontWeight
    {
        get => _selectedFontWeight;
        set
        {
            if (SetProperty(ref _selectedFontWeight, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string SelectedFontStyle
    {
        get => _selectedFontStyle;
        set
        {
            if (SetProperty(ref _selectedFontStyle, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string FontColorHex
    {
        get => _fontColorHex;
        set
        {
            if (SetProperty(ref _fontColorHex, value))
            {
                NotifyValidationChanged();
                QueueAutoSave();
            }
        }
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (SetProperty(ref _fontSize, value))
            {
                NotifyValidationChanged();
                QueueAutoSave();
            }
        }
    }

    public bool IsUnderline
    {
        get => _isUnderline;
        set
        {
            if (SetProperty(ref _isUnderline, value))
            {
                QueueAutoSave();
            }
        }
    }

    public bool AlarmEnabled
    {
        get => _alarmEnabled;
        set
        {
            if (SetProperty(ref _alarmEnabled, value))
            {
                NotifyValidationChanged();
                QueueAutoSave();
            }
        }
    }

    public DateTime? AlarmLocalDate
    {
        get => _alarmLocalDate;
        set
        {
            if (SetProperty(ref _alarmLocalDate, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string AlarmTimeText
    {
        get => _alarmTimeText;
        set
        {
            if (SetProperty(ref _alarmTimeText, value))
            {
                NotifyValidationChanged();
                QueueAutoSave();
            }
        }
    }

    public RepeatType SelectedRepeatType
    {
        get => _selectedRepeatType;
        set
        {
            if (SetProperty(ref _selectedRepeatType, value))
            {
                QueueAutoSave();
            }
        }
    }

    public DateTime? RepeatEndLocalDate
    {
        get => _repeatEndLocalDate;
        set
        {
            if (SetProperty(ref _repeatEndLocalDate, value))
            {
                QueueAutoSave();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string MarkdownPreviewHtml
    {
        get => _markdownPreviewHtml;
        set => SetProperty(ref _markdownPreviewHtml, value);
    }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (SetProperty(ref _searchKeyword, value))
            {
                RefreshNotesFilter();
            }
        }
    }

    public bool HasSelectedGroup => SelectedGroup != null;
    public bool HasSelectedNote => SelectedNote != null;
    public int FilteredNoteCount => FilteredNotesView?.Cast<object>().Count() ?? 0;
    public bool HasValidationErrors =>
        !string.IsNullOrEmpty(this[nameof(FontSize)]) ||
        !string.IsNullOrEmpty(this[nameof(FontColorHex)]) ||
        !string.IsNullOrEmpty(this[nameof(AlarmTimeText)]);

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(FontSize):
                    return ValidateFontSize();
                case nameof(FontColorHex):
                    return ValidateFontColorHex();
                case nameof(AlarmTimeText):
                    return ValidateAlarmTimeText();
                default:
                    return string.Empty;
            }
        }
    }

    public MainViewModel(
        IGroupService groupService,
        INoteService noteService,
        ITrashService trashService,
        IStickyNoteService stickyNoteService,
        ISettingsService settingsService,
        IAlarmService alarmService,
        IUserDialogService dialogService)
    {
        _groupService = groupService;
        _noteService = noteService;
        _trashService = trashService;
        _stickyNoteService = stickyNoteService;
        _settingsService = settingsService;
        _alarmService = alarmService;
        _dialogService = dialogService;
        _autoSaveDebouncer = new Debouncer(TimeSpan.FromMilliseconds(500));

        GroupTree = new ObservableCollection<GroupNodeViewModel>();
        AllGroupsFlat = new ObservableCollection<GroupNodeViewModel>();
        Notes = new ObservableCollection<NoteListItemViewModel>();
        TrashItems = new ObservableCollection<TrashItemViewModel>();
        FilteredNotesView = CollectionViewSource.GetDefaultView(Notes);
        FilteredNotesView.Filter = NoteMatchesSearch;

        FontFamilies = new[] { "Segoe UI", "Calibri", "Arial", "Consolas", "Malgun Gothic" };
        FontWeights = new[] { "Normal", "Bold" };
        FontStyles = new[] { "Normal", "Italic" };
        RepeatTypes = new[] { RepeatType.None, RepeatType.Daily, RepeatType.Weekly, RepeatType.Monthly };

        _noteTitle = string.Empty;
        _noteContent = string.Empty;
        _selectedFontFamily = "Segoe UI";
        _selectedFontWeight = "Normal";
        _selectedFontStyle = "Normal";
        _fontColorHex = "#000000";
        _fontSize = 14;
        _alarmTimeText = "09:00";
        _markdownPreviewHtml = BuildMarkdownHtml(string.Empty);
        _searchKeyword = string.Empty;

        InitializeCommand = new RelayCommand(Initialize);
        AddGroupCommand = new RelayCommand(AddGroup);
        RenameGroupCommand = new RelayCommand(RenameGroup);
        DeleteGroupCommand = new RelayCommand(DeleteGroup);
        AddNoteCommand = new RelayCommand(AddNote);
        DeleteNoteCommand = new RelayCommand(DeleteNote);
        MoveNoteCommand = new RelayCommand(MoveNote);
        OpenStickyCommand = new RelayCommand(OpenSticky);
        RestoreTrashItemCommand = new RelayCommand(RestoreTrashItem);
        EmptyTrashCommand = new RelayCommand(EmptyTrash);
        PurgeExpiredTrashCommand = new RelayCommand(PurgeExpiredTrash);
        RefreshCommand = new RelayCommand(RefreshAll);
        Snooze5Command = new RelayCommand(() => Snooze(5));
        Snooze10Command = new RelayCommand(() => Snooze(10));
        Snooze30Command = new RelayCommand(() => Snooze(30));
        DismissAlarmCommand = new RelayCommand(DismissAlarm);
        DeleteTrashItemPermanentlyCommand = new RelayCommand(DeleteTrashItemPermanently);
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        RefreshAll();
    }

    private void RefreshAll()
    {
        try
        {
            var prevGroupId = SelectedGroup?.Id;
            LoadGroupTree();

            if (prevGroupId.HasValue)
            {
                SelectedGroup = FindGroupById(prevGroupId.Value);
            }

            if (SelectedGroup == null)
            {
                SelectedGroup = GroupTree.FirstOrDefault() ?? AllGroupsFlat.FirstOrDefault();
            }

            LoadTrashItems();
            StatusMessage = "목록을 새로고침했습니다.";
        }
        catch (Exception ex)
        {
            _dialogService.Error("오류", ex.Message);
            StatusMessage = "새로고침에 실패했습니다.";
        }
    }

    private void LoadGroupTree()
    {
        var tree = _groupService.GetGroupTree();

        GroupTree.Clear();
        AllGroupsFlat.Clear();

        foreach (var dto in tree)
        {
            var node = MapGroupNode(dto);
            GroupTree.Add(node);
            Flatten(node, AllGroupsFlat);
        }
    }

    private GroupNodeViewModel MapGroupNode(GroupTreeNodeDto dto)
    {
        var node = new GroupNodeViewModel
        {
            Id = dto.Id,
            ParentGroupId = dto.ParentGroupId,
            Name = dto.Name,
            SortOrder = dto.SortOrder
        };

        foreach (var child in dto.Children.OrderBy(x => x.SortOrder).ThenBy(x => x.Name))
        {
            node.Children.Add(MapGroupNode(child));
        }

        return node;
    }

    private static void Flatten(GroupNodeViewModel node, ObservableCollection<GroupNodeViewModel> list)
    {
        list.Add(node);
        foreach (var child in node.Children)
        {
            Flatten(child, list);
        }
    }

    private GroupNodeViewModel FindGroupById(Guid id)
    {
        return AllGroupsFlat.FirstOrDefault(x => x.Id == id);
    }

    private void LoadNotesForSelectedGroup()
    {
        Notes.Clear();
        SelectedNote = null;

        if (SelectedGroup == null)
        {
            return;
        }

        var notes = _noteService.GetNotesByGroup(SelectedGroup.Id);
        foreach (var note in notes)
        {
            Notes.Add(new NoteListItemViewModel
            {
                Id = note.Id,
                Title = note.Title,
                AlarmEnabled = note.AlarmEnabled,
                UpdatedAtUtc = note.UpdatedAtUtc
            });
        }

        RefreshNotesFilter();
    }

    private void LoadEditorFromSelectedNote()
    {
        _suspendAutoSave = true;
        try
        {
            if (SelectedNote == null)
            {
                NoteTitle = string.Empty;
                NoteContent = string.Empty;
                MarkdownPreviewHtml = BuildMarkdownHtml(string.Empty);
                return;
            }

            var note = _noteService.GetNote(SelectedNote.Id);
            if (note == null)
            {
                return;
            }

            NoteTitle = note.Title;
            NoteContent = note.ContentMarkdown;
            MarkdownPreviewHtml = BuildMarkdownHtml(note.ContentMarkdown);
            SelectedFontFamily = note.FontFamily;
            SelectedFontWeight = note.FontWeight;
            SelectedFontStyle = note.FontStyle;
            FontColorHex = note.FontColorHex;
            FontSize = note.FontSize;
            IsUnderline = note.IsUnderline;
            AlarmEnabled = note.AlarmEnabled;
            SelectedRepeatType = note.RepeatType;
            RepeatEndLocalDate = note.RepeatEndUtc?.ToLocalTime().Date;

            var localAlarm = note.AlarmAtUtc?.ToLocalTime();
            AlarmLocalDate = localAlarm?.Date;
            AlarmTimeText = localAlarm.HasValue ? localAlarm.Value.ToString("HH:mm") : "09:00";
        }
        finally
        {
            _suspendAutoSave = false;
        }
    }

    private void QueueAutoSave()
    {
        if (_suspendAutoSave || SelectedNote == null)
        {
            return;
        }

        if (HasValidationErrors)
        {
            StatusMessage = "입력 값을 확인해주세요.";
            return;
        }

        _autoSaveDebouncer.Bounce(SaveSelectedNote);
    }

    private void SaveSelectedNote()
    {
        if (SelectedNote == null || HasValidationErrors)
        {
            return;
        }

        try
        {
            var request = BuildUpdateRequest(SelectedNote.Id);
            var updated = _noteService.UpdateNote(request);
            _alarmService.ScheduleOrUpdate(updated.Id);

            SelectedNote.Title = updated.Title;
            SelectedNote.AlarmEnabled = updated.AlarmEnabled;
            SelectedNote.UpdatedAtUtc = updated.UpdatedAtUtc;
            RefreshNotesFilter();
            StatusMessage = "메모를 저장했습니다.";
        }
        catch (Exception ex)
        {
            _dialogService.Error("저장 오류", ex.Message);
            StatusMessage = "메모 저장에 실패했습니다.";
        }
    }

    private UpdateNoteRequest BuildUpdateRequest(Guid noteId)
    {
        DateTime? alarmUtc = null;
        if (AlarmEnabled && AlarmLocalDate.HasValue)
        {
            var time = ParseTime(AlarmTimeText);
            var localDateTime = AlarmLocalDate.Value.Date.Add(time);
            alarmUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.Local);
        }

        DateTime? repeatEndUtc = null;
        if (RepeatEndLocalDate.HasValue)
        {
            var localEnd = RepeatEndLocalDate.Value.Date.AddHours(23).AddMinutes(59);
            repeatEndUtc = TimeZoneInfo.ConvertTimeToUtc(localEnd, TimeZoneInfo.Local);
        }

        return new UpdateNoteRequest
        {
            Id = noteId,
            Title = NoteTitle,
            ContentMarkdown = NoteContent,
            FontFamily = SelectedFontFamily,
            FontSize = FontSize,
            FontWeight = SelectedFontWeight,
            FontStyle = SelectedFontStyle,
            IsUnderline = IsUnderline,
            FontColorHex = FontColorHex,
            AlarmEnabled = AlarmEnabled,
            AlarmAtUtc = alarmUtc,
            TimeZoneId = TimeZoneInfo.Local.Id,
            RepeatType = SelectedRepeatType,
            RepeatEndUtc = repeatEndUtc
        };
    }

    private static string BuildMarkdownHtml(string markdown)
    {
        var rawHtml = Markdown.ToHtml(markdown ?? string.Empty);
        return "<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge\" />" +
               "<style>body{font-family:'Segoe UI';font-size:13px;padding:8px;line-height:1.5;}pre{background:#f4f4f4;padding:8px;}code{background:#f4f4f4;padding:2px 4px;}</style>" +
               "</head><body>" + rawHtml + "</body></html>";
    }

    private static TimeSpan ParseTime(string value)
    {
        if (TimeSpan.TryParseExact(value ?? string.Empty, "hh\\:mm", CultureInfo.InvariantCulture, out var time) &&
            time.TotalHours < 24)
        {
            return time;
        }

        throw new FormatException("알람 시간은 HH:mm 형식(00:00~23:59)이어야 합니다.");
    }

    private string ValidateFontSize()
    {
        if (FontSize <= 0 || FontSize > 200)
        {
            return "글꼴 크기는 1~200 사이여야 합니다.";
        }

        return string.Empty;
    }

    private string ValidateFontColorHex()
    {
        if (string.IsNullOrWhiteSpace(FontColorHex))
        {
            return "색상을 입력해주세요.";
        }

        if (!HexColorRegex.IsMatch(FontColorHex.Trim()))
        {
            return "색상은 #RRGGBB 또는 #AARRGGBB 형식이어야 합니다.";
        }

        return string.Empty;
    }

    private string ValidateAlarmTimeText()
    {
        if (!AlarmEnabled)
        {
            return string.Empty;
        }

        var value = AlarmTimeText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return "알람 시간을 입력해주세요.";
        }

        if (!TimeSpan.TryParseExact(value.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out var time) ||
            time.TotalHours >= 24)
        {
            return "알람 시간은 HH:mm 형식(00:00~23:59)이어야 합니다.";
        }

        return string.Empty;
    }

    private void NotifyValidationChanged()
    {
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private bool NoteMatchesSearch(object item)
    {
        if (item is not NoteListItemViewModel note)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            return true;
        }

        var keyword = SearchKeyword.Trim();
        return note.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RefreshNotesFilter()
    {
        FilteredNotesView.Refresh();
        OnPropertyChanged(nameof(FilteredNoteCount));

        if (SelectedNote != null && !FilteredNotesView.Cast<object>().Any(x => ReferenceEquals(x, SelectedNote)))
        {
            SelectedNote = null;
        }

        if (SelectedNote == null)
        {
            SelectedNote = FilteredNotesView.Cast<object>().OfType<NoteListItemViewModel>().FirstOrDefault();
        }
    }

    private void AddGroup()
    {
        var input = _dialogService.Prompt("그룹 생성", "그룹 이름", "새 그룹");
        if (input == null)
        {
            return;
        }

        _groupService.CreateGroup(new CreateGroupRequest
        {
            ParentGroupId = SelectedGroup?.Id,
            Name = input
        });

        RefreshAll();
        StatusMessage = "그룹을 생성했습니다.";
    }

    private void RenameGroup()
    {
        if (SelectedGroup == null)
        {
            return;
        }

        var input = _dialogService.Prompt("그룹 수정", "그룹 이름", SelectedGroup.Name);
        if (input == null)
        {
            return;
        }

        _groupService.UpdateGroup(new UpdateGroupRequest
        {
            Id = SelectedGroup.Id,
            Name = input
        });

        RefreshAll();
        StatusMessage = "그룹 이름을 수정했습니다.";
    }

    private void DeleteGroup()
    {
        if (SelectedGroup == null)
        {
            return;
        }

        if (SelectedGroup.Name == "Inbox")
        {
            _dialogService.Info("안내", "Inbox 그룹은 삭제할 수 없습니다.");
            return;
        }

        if (!_dialogService.Confirm("그룹 삭제", "선택한 그룹과 하위 메모를 휴지통으로 이동할까요?"))
        {
            return;
        }

        _groupService.SoftDeleteGroup(SelectedGroup.Id);
        RefreshAll();
        StatusMessage = "그룹을 휴지통으로 이동했습니다.";
    }

    private void AddNote()
    {
        try
        {
            var targetGroupId = SelectedGroup?.Id ?? _groupService.GetOrCreateInboxGroupId();
            var font = _settingsService.GetGlobalFontSetting();

            var created = _noteService.CreateNote(new CreateNoteRequest
            {
                GroupId = targetGroupId,
                Title = "새 메모",
                ContentMarkdown = string.Empty,
                FontFamily = font.FontFamily,
                FontSize = font.FontSize,
                FontWeight = font.FontWeight,
                FontStyle = font.FontStyle,
                FontColorHex = font.FontColorHex,
                IsUnderline = font.IsUnderline,
                AlarmEnabled = false,
                RepeatType = RepeatType.None,
                TimeZoneId = TimeZoneInfo.Local.Id
            });

            LoadNotesForSelectedGroup();
            SelectedNote = Notes.FirstOrDefault(x => x.Id == created.Id);
            StatusMessage = "메모를 생성했습니다.";
        }
        catch (Exception ex)
        {
            _dialogService.Error("메모 생성 오류", ex.Message);
            StatusMessage = "메모 생성에 실패했습니다.";
        }
    }

    private void DeleteNote()
    {
        if (SelectedNote == null)
        {
            return;
        }

        if (!_dialogService.Confirm("메모 삭제", "선택한 메모를 휴지통으로 이동할까요?"))
        {
            return;
        }

        _noteService.SoftDeleteNote(SelectedNote.Id);
        LoadNotesForSelectedGroup();
        LoadTrashItems();
        StatusMessage = "메모를 휴지통으로 이동했습니다.";
    }

    private void MoveNote()
    {
        if (SelectedNote == null || MoveTargetGroup == null)
        {
            return;
        }

        _noteService.MoveNote(SelectedNote.Id, MoveTargetGroup.Id);
        LoadNotesForSelectedGroup();
        StatusMessage = "메모를 다른 그룹으로 이동했습니다.";
    }

    private void OpenSticky()
    {
        if (SelectedNote == null)
        {
            return;
        }

        _stickyNoteService.OpenSticky(SelectedNote.Id);
    }

    private void Snooze(int minutes)
    {
        if (SelectedNote == null)
        {
            return;
        }

        _alarmService.Snooze(SelectedNote.Id, minutes);
        StatusMessage = $"알람을 {minutes}분 뒤로 스누즈했습니다.";
    }

    private void DismissAlarm()
    {
        if (SelectedNote == null)
        {
            return;
        }

        _alarmService.Dismiss(SelectedNote.Id);
        var refreshed = _noteService.GetNote(SelectedNote.Id);
        if (refreshed != null)
        {
            SelectedNote.AlarmEnabled = refreshed.AlarmEnabled;
        }

        StatusMessage = "알람을 해제했습니다.";
    }

    private void LoadTrashItems()
    {
        TrashItems.Clear();
        foreach (var item in _trashService.GetTrashItems())
        {
            TrashItems.Add(new TrashItemViewModel
            {
                Id = item.Id,
                ItemType = item.ItemType,
                Name = item.Name,
                DeletedAtUtc = item.DeletedAtUtc
            });
        }
    }

    private void RestoreTrashItem()
    {
        if (SelectedTrashItem == null)
        {
            return;
        }

        if (SelectedTrashItem.ItemType == TrashItemType.Note)
        {
            _trashService.RestoreNote(SelectedTrashItem.Id);
        }
        else
        {
            _trashService.RestoreGroup(SelectedTrashItem.Id);
        }

        RefreshAll();
        StatusMessage = "휴지통 항목을 복구했습니다.";
    }

    private void DeleteTrashItemPermanently()
    {
        if (SelectedTrashItem == null)
        {
            return;
        }

        if (!_dialogService.Confirm("영구 삭제", "선택한 휴지통 항목을 영구 삭제할까요?"))
        {
            return;
        }

        _trashService.DeleteItemPermanently(SelectedTrashItem.Id, SelectedTrashItem.ItemType);
        RefreshAll();
        StatusMessage = "휴지통 항목을 영구 삭제했습니다.";
    }

    private void EmptyTrash()
    {
        if (!_dialogService.Confirm("휴지통 비우기", "휴지통의 모든 항목을 영구 삭제할까요?"))
        {
            return;
        }

        _trashService.EmptyTrash();
        RefreshAll();
        StatusMessage = "휴지통을 비웠습니다.";
    }

    private void PurgeExpiredTrash()
    {
        _trashService.PurgeExpiredItems();
        RefreshAll();
        StatusMessage = "90일 경과 항목을 정리했습니다.";
    }
}
