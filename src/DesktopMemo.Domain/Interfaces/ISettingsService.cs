using DesktopMemo.Domain.Contracts;

namespace DesktopMemo.Domain.Interfaces;

public interface ISettingsService
{
    FontSettingDto GetGlobalFontSetting();
    void UpdateGlobalFontSetting(FontSettingDto dto);
}
