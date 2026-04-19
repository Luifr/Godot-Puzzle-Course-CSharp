using Game.AutoLoad;
using Game.UI;
using Godot;

public partial class MainMenu : Node
{
	[Export] private PackedScene optionMenuScene;

	private Button playButton;
	private Button optionsButton;
	private Button quitButton;
	private Control mainMenuContainer;
	private LevelSelectScreen levelSelectScreen;

	public override void _Ready()
	{
		levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");
		mainMenuContainer = GetNode<Control>("%MainMenuContainer");
		playButton = GetNode<Button>("%PlayButton");
		optionsButton = GetNode<Button>("%OptionsButton");
		quitButton = GetNode<Button>("%QuitButton");

		AudioHelpers.RegisterButtons([playButton, optionsButton, quitButton]);

		levelSelectScreen.Visible = false;
		mainMenuContainer.Visible = true;

		playButton.Pressed += OnPlayButtonPressed;
		optionsButton.Pressed += OnOptionsButtonPressed;
		levelSelectScreen.OnLevelSelectBackPressed += OnLevelSelectBackPressed;
		quitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnPlayButtonPressed()
	{
		levelSelectScreen.Visible = true;
		mainMenuContainer.Visible = false;
	}

	private void OnOptionsButtonPressed()
	{
		mainMenuContainer.Hide();
		var optionsMenu = optionMenuScene.Instantiate<OptionsMenu>();
		optionsMenu.DoneButtonPressed += () =>
		{
			mainMenuContainer.Show();
			optionsMenu.QueueFree();
		};
		AddChild(optionsMenu);
	}

	private void OnLevelSelectBackPressed()
	{
		levelSelectScreen.Visible = false;
		mainMenuContainer.Visible = true;
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
