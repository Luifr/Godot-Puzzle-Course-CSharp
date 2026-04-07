using Godot;

namespace Game.AutoLoad;

public partial class LevelManager : Node
{
	public static LevelManager Instance { get; private set; }

	[Export]
	private PackedScene[] LevelScenes;

	private int currentLevelIndex;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
	}

	public void ChangeToLevel(int levelIndex)
	{
		if (levelIndex >= LevelScenes.Length || levelIndex < 0)
		{
			GD.Print("LevelManager:ChangeToLevel Invalid level index. LevelIndex is " + levelIndex.ToString());
			return;
		}

		var levelScene = LevelScenes[levelIndex];

		GetTree().ChangeSceneToPacked(levelScene);
	}

	public void ChangeToNextLevel()
	{
		ChangeToLevel(++currentLevelIndex);
	}
}
