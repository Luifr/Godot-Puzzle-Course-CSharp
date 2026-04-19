using System.Collections.Generic;
using Godot;

namespace Game.AutoLoad;

public partial class AudioHelpers : Node
{
	private static AudioHelpers instance;

	private AudioStreamPlayer explosionAudioStreamPlayer;
	private AudioStreamPlayer clickAudioStreamPlayer;
	private AudioStreamPlayer victoryAudioStreamPlayer;
	private AudioStreamPlayer musicAudioStreamPlayer;

	public override void _EnterTree()
	{
		instance = this;
	}

	public override void _Ready()
	{
		explosionAudioStreamPlayer = GetNode<AudioStreamPlayer>("ExplosionAudioStreamPlayer");
		clickAudioStreamPlayer = GetNode<AudioStreamPlayer>("ClickAudioStreamPlayer");
		victoryAudioStreamPlayer = GetNode<AudioStreamPlayer>("VictoryAudioStreamPlayer");
		musicAudioStreamPlayer = GetNode<AudioStreamPlayer>("MusicAudioStreamPlayer");
	}

	public static void PlayVictory()
	{
		instance.victoryAudioStreamPlayer.Play();
	}

	public static void PlayBuildingDestruction()
	{
		instance.explosionAudioStreamPlayer.Play();
	}

	public static void RegisterButtons(IEnumerable<Button> buttons)
	{
		foreach (Button button in buttons)
		{
			button.Pressed += instance.OnButtonPressed;
		}
	}

	private void OnButtonPressed()
	{
		clickAudioStreamPlayer.Play();
	}
}
