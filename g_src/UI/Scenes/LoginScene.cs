using Godot;
using System.Net;
using System.Threading.Tasks;
using Client;
using Client.Networking;
using Client.Networking.Data;
using TazUOGodot.g_src.UI.Controls;

public partial class LoginScene : Control
{
	[Export] public TextureButton QuitButton { get; set; }
	[Export] public TextureButton ConnectButton { get; set; }
	
	private LineEdit _userName, _pass;
	
	private const string USERNAME_SAVE = "last_user_name";
	private const string PASSWORD_SAVE = "last_pass_saved";
	public static LoginScene Get()
	{
		return ResourceLoader.Load<PackedScene>("uid://cpmtkdd8s75pi").Instantiate() as LoginScene;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(QuitButton != null)
			QuitButton.Pressed += () => GetTree().Quit();
		
		Control c;
		AddChild(new Label(){Text = $"UO Version: To be discussed", Position = new(286, 465)}); //Version string
		
		//Check boxes that need to be turned into checkboxes
		UOGumpCheckbox r;
		AddChild(r = UOGumpCheckbox.Get(0x00D2, 0x00D3, true, "Auto login"));//Auto login
		r.Position = new(150, 417);
																	   
		AddChild(r = UOGumpCheckbox.Get(0x00D2, 0x00D3, true, "Save Account"));//Save Account
		r.Position = new(r.Position.X + r.GetSize().X + 10, 417);

		AddChild(c = new UONineSliceControl(0x0BB8, 210, 30) {Position = new(218, 283)});//Account bg
		AddChild(_userName = new LineEdit(){Position = c.Position, Size = c.Size});
		_userName.AddThemeColorOverride("font_color", new Color(0.1f, 0.1f, 0.1f));
		_userName.Alignment = HorizontalAlignment.Center;
		TazUO.Client.Instance.Settings.GetAsync(USERNAME_SAVE, string.Empty, (s) => { _userName?.SetText(s); });

		AddChild(c = new UONineSliceControl(0x0BB8, 210, 30) {Position = new(218, 333)});//Pass bg
		AddChild(_pass = new LineEdit(){Position = c.Position, Size = c.Size, Secret = true});
		_pass.AddThemeColorOverride("font_color", new Color(0.1f, 0.1f, 0.1f));
		_pass.Alignment = HorizontalAlignment.Center;
		TazUO.Client.Instance.Settings.GetAsync(PASSWORD_SAVE, string.Empty, (s) =>
		{
			if (!string.IsNullOrEmpty(s))
			{
				_pass?.SetText(ClassicUO.Utility.Crypter.Decrypt(s));
			}
		});
		
		if(ConnectButton != null && !Engine.IsEditorHint())
			ConnectButton.Pressed += () =>
			{
				if (string.IsNullOrEmpty(_userName.Text) || string.IsNullOrEmpty(_pass.Text)) return;
				
				TazUO.Client.Instance.Settings.SetAsync(USERNAME_SAVE, _userName.Text);
				TazUO.Client.Instance.Settings.SetAsync(PASSWORD_SAVE, ClassicUO.Utility.Crypter.Encrypt(_pass.Text));
				
				Network.Info = new ConnectInfo()
				{
					EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2593),
					Username = _userName.Text,
					Password = _pass.Text,
					Seed = 1
				};
				
				Assistant.Configure();
				TazUO.Client.Instance.ConnectionTask = Task.Run(Network.AsyncConnect);
			};
	}
}
