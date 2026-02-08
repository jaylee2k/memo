using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.App.ViewModels;

public class TrashItemViewModel : ObservableObject
{
    public Guid Id { get; set; }
    public TrashItemType ItemType { get; set; }

    private string _name;
    private DateTime? _deletedAtUtc;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTime? DeletedAtUtc
    {
        get => _deletedAtUtc;
        set
        {
            if (SetProperty(ref _deletedAtUtc, value))
            {
                OnPropertyChanged(nameof(DeletedText));
            }
        }
    }

    public string ItemTypeText => ItemType == TrashItemType.Group ? "그룹" : "메모";
    public string DeletedText => DeletedAtUtc.HasValue ? DeletedAtUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : string.Empty;

    public TrashItemViewModel()
    {
        _name = string.Empty;
    }
}
