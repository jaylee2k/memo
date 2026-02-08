using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopMemo.App.ViewModels;

public class GroupNodeViewModel : ObservableObject
{
    private string _name;

    public Guid Id { get; set; }
    public Guid? ParentGroupId { get; set; }
    public int SortOrder { get; set; }
    public ObservableCollection<GroupNodeViewModel> Children { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public GroupNodeViewModel()
    {
        _name = string.Empty;
        Children = new ObservableCollection<GroupNodeViewModel>();
    }
}
