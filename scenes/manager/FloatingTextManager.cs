using Game.UI;
using Godot;

namespace Game.Manager;

public partial class FloatingTextManager : Node
{
	[Export] PackedScene floatingTextScene;

	private static FloatingTextManager instance;

	public override void _EnterTree()
	{
		instance = this;
	}

	public override void _Ready()
	{
	}

	public static void ShowMessage(string message)
	{
		var floatingText = instance.floatingTextScene.Instantiate<FloatingText>();
		instance.AddChild(floatingText);
		floatingText.SetText(message);
		floatingText.GlobalPosition = floatingText.GetGlobalMousePosition();
	}
}
