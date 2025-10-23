using Godot;
using System;

public partial class AskForInput : Control
{
	private string _title = string.Empty;
	private Action<bool, string> _result;
	private string _cancelButton = "Cancel";
	private string _acceptButton = "Accept";

	public AskForInput()
	{
		
	}

	public static AskForInput Get(string title, Action<bool, string> result, string cancelButton = "Cancel", string acceptButton = "Accept")
	{
		AskForInput aai = ResourceLoader.Load<PackedScene>("uid://dqx661ecptvmg").Instantiate() as AskForInput;
		
		aai._title = title;
		aai._result = result;
		aai._cancelButton = cancelButton;
		aai._acceptButton = acceptButton;
		aai._title = title;
		
		return aai;
	}

	[Export] public Label Label { get; set; }
	[Export] public Button CancelButton { get; set; }
	[Export] public Button OkButton { get; set; }
	[Export] public LineEdit Input { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Label != null)
			Label.Text = _title;
		
		if (CancelButton != null)
		{
			CancelButton.Text = _cancelButton;
			CancelButton.Pressed += CancelButtonOnPressed;
		}
		
		if (OkButton != null)
		{
			OkButton.Text = _acceptButton;
			OkButton.Pressed += OkButtonOnPressed;
		}
	}

	private void OkButtonOnPressed()
	{
		_result.Invoke(true, Input.Text);
		QueueFree();
	}

	private void CancelButtonOnPressed()
	{
		_result?.Invoke(false, Input.Text);
		QueueFree();
	}
}
