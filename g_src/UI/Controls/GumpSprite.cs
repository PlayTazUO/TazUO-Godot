using Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;


[Tool]
public partial class GumpSprite : TextureRect
{
	private ushort _graphic;

	[Export] public ushort Graphic
	{
		get => _graphic;
		set
		{
			_graphic = value;
			Texture = AssetHelper.GetGumpTexture(_graphic);
			if (Texture != null)
			{
				var size = Texture.GetSize();
				Width = (int)size.X;
				Height = (int)size.Y;
			}
		}
	}

	public int Width { get; private set; }

	public int Height { get; private set; }
}
