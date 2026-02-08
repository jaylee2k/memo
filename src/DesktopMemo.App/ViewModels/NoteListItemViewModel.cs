using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopMemo.App.ViewModels;

public class NoteListItemViewModel : ObservableObject
{
    private string _title;
    private DateTime _updatedAtUtc;
    private bool _alarmEnabled;

    public Guid Id { get; set; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public DateTime UpdatedAtUtc
    {
        get => _updatedAtUtc;
        set
        {
            if (SetProperty(ref _updatedAtUtc, value))
            {
                OnPropertyChanged(nameof(UpdatedText));
            }
        }
    }

    public bool AlarmEnabled
    {
        get => _alarmEnabled;
        set => SetProperty(ref _alarmEnabled, value);
    }

    public string UpdatedText => UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public NoteListItemViewModel()
    {
        _title = string.Empty;
    }
}
