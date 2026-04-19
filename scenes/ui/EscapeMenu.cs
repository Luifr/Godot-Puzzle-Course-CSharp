using Game.AutoLoad;
using Godot;

namespace Game.UI;

public partial class EscapeMenu : CanvasLayer
{
	[Export(PropertyHint.File, "*.tscn")]
	private string mainMenuScenePath;
	[Export] private PackedScene optionsMenuScene;

	private Button resumeButton;
	private Button optionsButton;
	private Button quitButton;
	private MarginContainer marginContainer;

	public override void _Ready()
	{
		resumeButton = GetNode<Button>("%ResumeButton");
		optionsButton = GetNode<Button>("%OptionsButton");
		quitButton = GetNode<Button>("%QuitToMenuButton");
		marginContainer = GetNode<MarginContainer>("MarginContainer");

		AudioHelpers.RegisterButtons([
			resumeButton, optionsButton, quitButton
		]);

		resumeButton.Pressed += OnResumePressed;
		optionsButton.Pressed += OnOptionsPressed;
		quitButton.Pressed += OnQuitPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("escape"))
		{
			QueueFree();
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnResumePressed()
	{
		QueueFree();
	}

	private void OnOptionsPressed()
	{
		marginContainer.Hide();
		var optionsMenu = optionsMenuScene.Instantiate<OptionsMenu>();
		optionsMenu.DoneButtonPressed += () =>
		{
			marginContainer.Show();
			optionsMenu.QueueFree();
		};
		AddChild(optionsMenu);
	}

	private void OnQuitPressed()
	{
		GetTree().ChangeSceneToFile(mainMenuScenePath);
	}
}
