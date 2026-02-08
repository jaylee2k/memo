namespace DesktopMemo.Domain.Contracts;

public class FontSettingDto
{
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public string FontWeight { get; set; }
    public string FontStyle { get; set; }
    public bool IsUnderline { get; set; }
    public string FontColorHex { get; set; }

    public FontSettingDto()
    {
        FontFamily = "Segoe UI";
        FontSize = 14;
        FontWeight = "Normal";
        FontStyle = "Normal";
        FontColorHex = "#000000";
    }
}
