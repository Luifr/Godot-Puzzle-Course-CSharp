using Game.AutoLoad;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
	[Signal] public delegate void OnLevelSelectBackPressedEventHandler();

	[Export]
	private PackedScene levelSeelctSectionScene;

	private GridContainer gridContainer;
	private Button backButton;

	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("%GridContainer");
		backButton = GetNode<Button>("BackButton");

		backButton.Pressed += EmitSignalOnLevelSelectBackPressed;

		var levelDefinitions = LevelManager.GetLevelDefinitions();

		for (var i = 0; i < levelDefinitions.Length; i++)
		{
			var levelDefinition = levelDefinitions[i];

			var levelSelectionScene = levelSeelctSectionScene.Instantiate<LevelSelectSection>();
			gridContainer.AddChild(levelSelectionScene);

			levelSelectionScene.SetLevelDefinition(levelDefinition);
			levelSelectionScene.SetLevelIndex(i);
			levelSelectionScene.LevelSelected += OnLevelSelected;
		}
	}

	private void OnLevelSelected(int index)
	{
		LevelManager.Instance.ChangeToLevel(index);
	}
}
