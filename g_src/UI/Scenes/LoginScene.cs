using Godot;
using System;
using TazUOGodot.g_src.UI.Controls;
using TazUO.Godot.Utility;

public partial class LoginScene : Control
{
	public static LoginScene Get()
	{
		return ResourceLoader.Load<PackedScene>("uid://cpmtkdd8s75pi").Instantiate() as  LoginScene;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var rect = GetRect();
		AddChild(new TiledGumpSprite(0x0E14, (int)rect.Size.X, (int)rect.Size.Y)); //Tiled BG

		AddChild(new GumpSprite(0x014E)); //Background

		UOGumpButton button;
		AddChild(button = UOGumpButton.Get(0x05CA, 0x05C9, 0x05C8)); //Quit
		button.Position = new Vector2(25, 240);
		button.Pressed += () => GetTree()?.Quit();
		
		Control c;
		AddChild(c = UOGumpButton.Get(0x05D0, 0x05CF, 0x5CE)); //Credits
		c.Position = new Vector2(530, 125);
		
		AddChild(c = UOGumpButton.Get(0x5CD, 0x5CC, 0x5CB)); //Arrow
		c.Position = new Vector2(280, 365);
		
		AddChild(new Label(){Text = $"UO Version: {Client.Instance.FileManager.Version}", Position = new(286, 465)}); //Version string
		
		//Check boxes that need to be turned into checkboxes
		UOGumpCheckbox r;
		AddChild(r = UOGumpCheckbox.Get(0x00D2, 0x00D3, true, "Auto login"));//Auto login
		r.Position = new(150, 417);
																	   
		AddChild(r = UOGumpCheckbox.Get(0x00D2, 0x00D3, true, "Save Account"));//Save Account
		r.Position = new(r.Position.X + r.GetSize().X + 10, 417);

		LineEdit l;
		AddChild(c = new UONineSliceControl(0x0BB8, 210, 30) {Position = new(218, 283)});//Account bg
		AddChild(l = new LineEdit(){Position = c.Position, Size = c.Size});
		l.AddThemeColorOverride("font_color", new Color(0.1f, 0.1f, 0.1f));
		l.Alignment = HorizontalAlignment.Center;

		AddChild(c = new UONineSliceControl(0x0BB8, 210, 30) {Position = new(218, 333)});//Pass bg
		AddChild(l = new LineEdit(){Position = c.Position, Size = c.Size});
		l.AddThemeColorOverride("font_color", new Color(0.1f, 0.1f, 0.1f));
		l.Alignment = HorizontalAlignment.Center;
	}
}
