using Godot;
using System;
using System.IO;
using ClassicUO.Assets;
using ClassicUO.Utility;
using TazUO.Godot.Utility;

public partial class Client : Node
{
	[Export] public CanvasLayer UILayer { get; set; }
	
	public static Client Instance;
	public UOFileManager  FileManager;
	public SQLSettingsManager Settings;
	public string UserPath;
	
	private const string UOPATHSAVE = "UOPATHSAVED";

	public Client()
	{
		if (OS.HasFeature("editor"))
			UserPath = ProjectSettings.GlobalizePath("user://");
		else
			UserPath = OS.GetExecutablePath().PathJoin("Data");
		
		Settings = new(UserPath);
		
		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();
		
		string uoPath = Settings.Get(UOPATHSAVE);

		if (string.IsNullOrEmpty(uoPath))
		{
			UILayer.AddChild(AskForInput.Get("Please enter the path to the UO file.", (b, s) =>
			{
				if (!b) return;
				
				SetUOPath(s);
				LoadFileManager(s);
			}));
		}
		else
		{
			LoadFileManager(uoPath);
		}
	}

	private void LoadFileManager(string path, string ver = "7.0.110.48")
	{
		if (string.IsNullOrEmpty(path)) return;

		if (!Path.Exists(path)) return;

		if (ClientVersionHelper.TryParseFromFile(path.PathJoin("client.exe"), out var v))
			ver = v;
		
		if(ClientVersionHelper.IsClientVersionValid(ver, out var version))
		{
			FileManager = new UOFileManager(version, path);
			FileManager.Load(false, "en");
			
			UILayer.AddChild(LoginScene.Get());
		}
	}
	
	private void SetUOPath(string path)
	{
		Settings?.SetAsync(UOPATHSAVE, path);
	}
}
