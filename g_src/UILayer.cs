using Godot;
using System;

public partial class UILayer : CanvasLayer
{
	public static UILayer Instance;

	public UILayer()
	{
		Instance = this;
	}
}
