using System.Linq;
using Game.Resources.Level;
using Godot;

namespace Game.AutoLoad;

public partial class LevelManager : Node
{
	private static LevelManager instance;

	[Export]
	private LevelDefinitionResource[] levelDefinitions;

	private static int currentLevelIndex;

	public override void _EnterTree()
	{
		instance = this;
	}

	public static LevelDefinitionResource[] GetLevelDefinitions()
	{
		return instance.levelDefinitions.ToArray();
	}

	public static void ChangeToLevel(int levelIndex)
	{
		if (levelIndex >= instance.levelDefinitions.Length || levelIndex < 0)
		{
			GD.PushError("LevelManager:ChangeToLevel Invalid level index. LevelIndex is " + levelIndex.ToString());
			return;
		}

		currentLevelIndex = levelIndex;

		var levelDefinition = instance.levelDefinitions[currentLevelIndex];

		instance.GetTree().ChangeSceneToFile(levelDefinition.LevelScenePath);
	}

	public static void ChangeToNextLevel()
	{
		ChangeToLevel(currentLevelIndex + 1);
	}

	public static bool IsLastLevel()
	{
		return currentLevelIndex == instance.levelDefinitions.Length - 1;
	}
}
