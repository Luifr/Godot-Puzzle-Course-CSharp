using Game.AutoLoad;
using Godot;
using System;

public partial class MainMenu : Node
{
	private Button playButton;

	public override void _Ready()
	{
		playButton = GetNode<Button>("%PlayButton");

		playButton.Pressed += OnPlayButtonPressed;
	}

	private void OnPlayButtonPressed()
	{
		LevelManager.Instance.ChangeToLevel(0);
	}
}
