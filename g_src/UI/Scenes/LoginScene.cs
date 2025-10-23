using Godot;
using System;
using TazUOGodot.g_src.UI.Controls;

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
		GumpSprite r;
		AddChild(r = new GumpSprite(0x00D2) {Position = new(150, 417)});//Auto login
		AddChild(new GumpSprite(0x00D2) {Position = new(r.Position.X + r.Width + 10, 417)});//Save Account

		AddChild(new GumpSprite(0x0BB8) {Position = new(218, 283), Scale = new Vector2(3, 1)});//Account bg, needs to be stretched
		AddChild(new GumpSprite(0x0BB8) {Position = new(218, 333), Scale = new Vector2(3, 1)});//pass bg, needs to be stretched

	}
}
