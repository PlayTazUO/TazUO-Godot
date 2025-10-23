using System;
using System.Runtime.InteropServices;
using Godot;
using TazUO.Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;

public partial class GumpSprite : Sprite2D
{
	private readonly ushort _graphic;
	public int Width { get; private set; }
	public int Height { get; private set; }

	public GumpSprite(ushort graphic)
	{
		_graphic = graphic;

		Texture = AssetHelper.GetGumpTexture(graphic);
		if (Texture != null)
		{
			var size = Texture.GetSize();
			Width = (int)size.X;
			Height = (int)size.Y;
		}

		Centered = false;
	}
}
