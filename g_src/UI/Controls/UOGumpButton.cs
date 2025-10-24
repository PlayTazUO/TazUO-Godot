using System.Text;
using Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;

public partial class UOGumpButton : TextureButton
{
	public static UOGumpButton Get(ushort normal, ushort pressed, ushort hover)
	{
		UOGumpButton button = new();

		button.TextureNormal = AssetHelper.GetGumpTexture(normal);
		button.TexturePressed = AssetHelper.GetGumpTexture(pressed);
		button.TextureHover = AssetHelper.GetGumpTexture(hover);
		
		return button;
	}
}
