using Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;

public partial class TiledGumpSprite : TextureRect
{
    private readonly ushort _graphic;
    private readonly int _width;
    private readonly int _height;

    public TiledGumpSprite(ushort graphic, int width, int height)
    {
        _graphic = graphic;
        _width = width;
        _height = height;

        Texture = AssetHelper.GetGumpTexture(graphic, true);
        StretchMode = StretchModeEnum.Tile;
        Size = new Vector2(_width, _height);
        GD.Print($"Expected: {_width}x{_height}, real: {Size.X}x{Size.Y}");
    }
}
