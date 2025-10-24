using Godot;
using System;
using TazUO.Godot.Utility;
using TazUOGodot.g_src.UI.Controls;

public partial class UOGumpCheckbox : BaseButton
{
	public bool Checked { get; private set; }
	
	private readonly Texture2D _inactive;
	private readonly Texture2D _active;
	private readonly string _text;
	private readonly ushort _hue;
	private readonly TextureRect _sprite;
	private readonly Label _label;

	public static UOGumpCheckbox Get(ushort inactive, ushort active, bool isChecked, string text = "", ushort hue = 0)
	{
		return new(AssetHelper.GetGumpTexture(inactive), AssetHelper.GetGumpTexture(active), isChecked, text, hue);
	}

	private UOGumpCheckbox(Texture2D inactive, Texture2D active, bool isChecked, string text = "", ushort hue = 0)
	{
		_inactive = inactive;
		_active = active;
		_text = text;
		_hue = hue;
		Checked = isChecked;

		if (inactive == null || active == null)
			return;

		var cbWidth = Math.Max(inactive.GetWidth(), active.GetWidth());
		var cbHeight = Math.Max(inactive.GetHeight(), active.GetHeight());
		
		_sprite = new TextureRect() {Texture = isChecked ? active : inactive};
		_sprite.MouseFilter = MouseFilterEnum.Pass;
		
		AddChild(_sprite);
		
		if (!string.IsNullOrEmpty(text))
		{
			AddChild(_label = new (){ Text = text});
			_label.MouseFilter = MouseFilterEnum.Pass;
			_label.Position = new (cbWidth + 5, 0);
			//Needs some work for vertical centering
		}
	}

	public override void _Pressed()
	{
		base._Pressed();
		
		Checked = !Checked;
		_sprite.Texture = Checked ? _active : _inactive;
	}
}
