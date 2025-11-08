using Godot;
using System.IO;
using System.Runtime.CompilerServices;
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

[Tool]
public partial class Client : Node
{
	[Export] public CanvasLayer UILayer { get; set; }
	
	public static Client Instance;
	public UOFileManager  FileManager;
	public SQLSettingsManager Settings;
	public string UserPath;
	public string ClientVersion;
	public Task ConnectionTask;
	
	/// <summary>
	/// For editor use, set this to your UO file path. Don't commit it, will be fixed later, I know it's stupid for now
	/// </summary>
	public const string EDITOR_ONLY_DATA_PATH = "/home/tazman/UO/UOAlive 7.0.110.48/";
	
	private const string UOPATHSAVE = "UOPATHSAVED";

	private EventListeners _listeners;

	public Client()
	{
		if (Engine.IsEditorHint())
			UserPath = ProjectSettings.GlobalizePath("user://");
		else
			UserPath = Path.GetDirectoryName(OS.GetExecutablePath()).PathJoin("Data");
		
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
				UILayer.AddChild(LoginScene.Get());
			}));
		}
		else
		{
			LoadFileManager(uoPath);
			UILayer.AddChild(LoginScene.Get());
		}
	}

	public UOFileManager GetFileManager()
	{
		if (FileManager != null) return FileManager;
		
		LoadFileManager(UserPath);
		return FileManager;
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
		}
	}
	
	private void SetUOPath(string path)
	{
		Settings?.SetAsync(UOPATHSAVE, path);
	}
}
