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
		AddChild(new TiledGumpSprite(0x0E14, (int)rect.Size.X, (int)rect.Size.Y));

		AddChild(new GumpSprite(0x014E));

		AddChild(new GumpSprite(0x05CA){Position = new Vector2(25, 240)}); //Quit

		AddChild(new GumpSprite(0x05D0){Position = new Vector2(530, 125)}); //Credits
		AddChild(new GumpSprite(0x5CD){Position = new Vector2(280, 365)}); //Arrow
		AddChild(new Label(){Text = $"UO Version: {Client.Instance.FileManager.Version}", Position = new(286, 465)}); //Version string
		
		//Check boxes that need to be turned into checkboxes
		GumpSprite r;
		AddChild(r = new GumpSprite(0x00D2) {Position = new(150, 417)});//Auto login
		AddChild(new GumpSprite(0x00D2) {Position = new(r.Position.X + r.Width + 10, 417)});//Save Account

		UONineSliceControl c;
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
