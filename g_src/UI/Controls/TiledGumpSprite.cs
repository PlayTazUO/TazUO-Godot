using Godot;
using TazUO.Godot.Utility;

[Tool]
public partial class TiledGumpSprite : TextureRect
{
	private ushort _graphic;
	private int _width;
	private int _height;

	[Export] public ushort Graphic
	{
		get => _graphic;
		set
		{
			_graphic = value;
			Texture = AssetHelper.GetGumpTexture(_graphic, true);
		}
	}

	[Export] public int Width
	{
		get => _width;
		set 
		{
			_width = value;
			Size = new Vector2(_width, _height);
		}
	}

	[Export] public int Height
	{
		get => _height;
		set
		{
			_height = value;
			Size = new Vector2(_width, _height);
		}
	}

	public TiledGumpSprite()
	{
		StretchMode = StretchModeEnum.Tile;
	}
}
