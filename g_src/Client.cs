using Godot;
using System.IO;
using System.Threading.Tasks;
using ClassicUO.Assets;
using ClassicUO.Utility;
using Client;
using Client.Networking;
using Client.Networking.Arguments;
using Client.Networking.Data;
using TazUO.Godot.Utility;
using TazUOGodot.g_src;
using Logger = Client.Logger;

namespace TazUO;

public partial class Client : Node
{
	[Export] public CanvasLayer UILayer { get; set; }
	
	public static Client Instance;
	public UOFileManager  FileManager;
	public SQLSettingsManager Settings;
	public string UserPath;
	public string ClientVersion;
	public Task ConnectionTask;
	
	private const string UOPATHSAVE = "UOPATHSAVED";

	private EventListeners _listeners;

	public Client()
	{
		if (OS.HasFeature("editor"))
			UserPath = ProjectSettings.GlobalizePath("user://");
		else
			UserPath = OS.GetExecutablePath().PathJoin("Data");
		
		Settings = new(UserPath);
		
		Application.Instance = Instance = this;
		
		ConfigLogger();
		_listeners = new();
	}

	#region Logging
	private void ConfigLogger()
	{
		Logger.OnLog += LoggerOnOnLog;
		Logger.OnPushWarning += LoggerOnOnPushWarning;
		Logger.OnLogError += LoggerOnOnLogError;
	}

	private void LoggerOnOnLogError(object sender, string e)
	{
		GD.PrintErr(e);
	}

	private void LoggerOnOnPushWarning(object sender, string e)
	{
		GD.Print("WARNING: " + e);
	}

	private void LoggerOnOnLog(object sender, string e)
	{
		GD.Print(e);
	}
	#endregion
	
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
		
		ClientVersion = ver;
		
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
