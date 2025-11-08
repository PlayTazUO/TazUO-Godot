using System.Text;
using Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;

[Tool]
public partial class UOGumpButton : TextureButton
{
	private ushort _graphic;
	private ushort _graphicPressed;
	private ushort _graphicHover;

	[Export] public ushort GraphicNormal
	{
		get => _graphic;
		set
		{
			_graphic = value;
			TextureNormal = AssetHelper.GetGumpTexture(_graphic);
		}
	}
	
	[Export] public ushort GraphicPressed
	{
		get => _graphicPressed;
		set
		{
			_graphicPressed = value;
			TexturePressed = AssetHelper.GetGumpTexture(_graphicPressed);
		}
	}
	
	[Export] public ushort GraphicHover
	{
		get => _graphicHover;
		set
		{
			_graphicHover = value;
			TextureHover = AssetHelper.GetGumpTexture(_graphicHover);
		}
	}

	public UOGumpButton()
	{
		
	}
	
	public static UOGumpButton Get(ushort normal, ushort pressed, ushort hover)
	{
		UOGumpButton button = new();

		button.TextureNormal = AssetHelper.GetGumpTexture(normal);
		button.TexturePressed = AssetHelper.GetGumpTexture(pressed);
		button.TextureHover = AssetHelper.GetGumpTexture(hover);
		
		return button;
	}
}
