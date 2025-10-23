using Godot;
using System;
using ClassicUO.Assets;
using ClassicUO.Utility;

public partial class Client : Node
{
	public static Client Instance;
	public UOFileManager  fileManager;

	public Client()
	{
		Instance = this;
		fileManager = new UOFileManager(ClientVersion.CV_7010400, "/home/tazman/UO/UOAlive 7.0.110.48/");
		fileManager.Load(false, "en");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
