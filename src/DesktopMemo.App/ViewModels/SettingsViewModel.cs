using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopMemo.App.Services;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.App.ViewModels;

public class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IUserDialogService _dialogService;

    private string _fontFamily;
    private double _fontSize;
    private string _fontWeight;
    private string _fontStyle;
    private bool _isUnderline;
    private string _fontColorHex;

    public IRelayCommand SaveCommand { get; }

    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    public string FontWeight
    {
        get => _fontWeight;
        set => SetProperty(ref _fontWeight, value);
    }

    public string FontStyle
    {
        get => _fontStyle;
        set => SetProperty(ref _fontStyle, value);
    }

    public bool IsUnderline
    {
        get => _isUnderline;
        set => SetProperty(ref _isUnderline, value);
    }

    public string FontColorHex
    {
        get => _fontColorHex;
        set => SetProperty(ref _fontColorHex, value);
    }

    public SettingsViewModel(ISettingsService settingsService, IUserDialogService dialogService)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;

        SaveCommand = new RelayCommand(Save);

        var current = _settingsService.GetGlobalFontSetting();
        _fontFamily = current.FontFamily;
        _fontSize = current.FontSize;
        _fontWeight = current.FontWeight;
        _fontStyle = current.FontStyle;
        _isUnderline = current.IsUnderline;
        _fontColorHex = current.FontColorHex;
    }

    private void Save()
    {
        _settingsService.UpdateGlobalFontSetting(new FontSettingDto
        {
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            IsUnderline = IsUnderline,
            FontColorHex = FontColorHex
        });

        _dialogService.Info("저장 완료", "전체 폰트 설정을 저장했습니다.");
    }
}
