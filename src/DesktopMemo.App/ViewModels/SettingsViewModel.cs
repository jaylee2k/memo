using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopMemo.App.Services;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Interfaces;

namespace DesktopMemo.App.ViewModels;

public class SettingsViewModel : ObservableObject, IDataErrorInfo
{
    private static readonly Regex HexColorRegex = new Regex("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$");

    private readonly ISettingsService _settingsService;
    private readonly IUserDialogService _dialogService;

    private string _fontFamily;
    private double _fontSize;
    private string _fontWeight;
    private string _fontStyle;
    private bool _isUnderline;
    private string _fontColorHex;

    public IRelayCommand SaveCommand { get; }
    public bool HasValidationErrors =>
        !string.IsNullOrEmpty(this[nameof(FontSize)]) ||
        !string.IsNullOrEmpty(this[nameof(FontColorHex)]);

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
                default:
                    return string.Empty;
            }
        }
    }

    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (SetProperty(ref _fontSize, value))
            {
                NotifyValidationChanged();
            }
        }
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
        set
        {
            if (SetProperty(ref _fontColorHex, value))
            {
                NotifyValidationChanged();
            }
        }
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
        if (HasValidationErrors)
        {
            _dialogService.Error("입력 오류", "글꼴 크기와 색상 값을 확인해주세요.");
            return;
        }

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

    private void NotifyValidationChanged()
    {
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(HasValidationErrors));
    }
}
