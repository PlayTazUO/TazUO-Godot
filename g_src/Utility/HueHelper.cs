using Godot;

namespace TazUO.Godot.Utility;

public static class HueHelper
{
    public static Color ConvertHueToColor(int hue)
    {
        if (hue == 0xFFFF || hue == ushort.MaxValue)
        {
            return new Color(1, 1, 1);
        }

        if (hue == 0)
            hue = 946; //Change black text to standard gray

        uint color = Client.Instance.FileManager.Hues.GetHueColorRgba8888(31, (ushort)hue);

        // Extract RGBA components from packed uint and convert to Godot.Color
        return Color.Color8(
            (byte)((color >> 24) & 0xFF),  // R
            (byte)((color >> 16) & 0xFF),  // G
            (byte)((color >> 8) & 0xFF),   // B
            (byte)(color & 0xFF)            // A
        );
    }
}