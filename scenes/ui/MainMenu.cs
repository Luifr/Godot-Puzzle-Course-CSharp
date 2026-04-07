using Game.UI;
using Godot;

public partial class MainMenu : Node
{
	private Button playButton;
	private Button quitButton;
	private Control mainMenuContainer;
	private LevelSelectScreen levelSelectScreen;

	public override void _Ready()
	{
		levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");
		mainMenuContainer = GetNode<Control>("%MainMenuContainer");
		playButton = GetNode<Button>("%PlayButton");
		quitButton = GetNode<Button>("%QuitButton");

		levelSelectScreen.Visible = false;
		mainMenuContainer.Visible = true;

		playButton.Pressed += OnPlayButtonPressed;
		levelSelectScreen.OnLevelSelectBackPressed += OnLevelSelectBackPressed;
		quitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnPlayButtonPressed()
	{
		levelSelectScreen.Visible = true;
		mainMenuContainer.Visible = false;
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
