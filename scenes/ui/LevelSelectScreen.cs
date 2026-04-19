using Game.AutoLoad;
using Game.Resources.Level;
using Godot;

namespace Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
	private const int PAGE_SIZE = 6;

	[Signal] public delegate void OnLevelSelectBackPressedEventHandler();

	[Export]
	private PackedScene levelSeelctSectionScene;

	private GridContainer gridContainer;
	private Button backButton;
	private int pageIndex;
	private int maxPageIndex;
	private LevelDefinitionResource[] levelDefinitions;

	private Button nextPageButton;
	private Button previousPageButton;

	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("%GridContainer");
		backButton = GetNode<Button>("BackButton");
		nextPageButton = GetNode<Button>("%NextPageButton");
		previousPageButton = GetNode<Button>("%PreviousPageButton");

		AudioHelpers.RegisterButtons([nextPageButton, previousPageButton, backButton]);

		backButton.Pressed += EmitSignalOnLevelSelectBackPressed;
		nextPageButton.Pressed += () => OnPageChanged(1);
		previousPageButton.Pressed += () => OnPageChanged(-1);

		levelDefinitions = LevelManager.GetLevelDefinitions();

		maxPageIndex = (int)Mathf.Ceil(levelDefinitions.Length / (float)PAGE_SIZE) - 1;

		ShowPage();
	}

	private void ShowPage()
	{
		foreach (var child in gridContainer.GetChildren()) child.QueueFree();

		UpdateButtonVisibility();

		var startIndex = PAGE_SIZE * pageIndex;
		var endIndex = Mathf.Min(PAGE_SIZE + startIndex, levelDefinitions.Length);
		for (var i = startIndex; i < endIndex; i++)
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

	private void UpdateButtonVisibility()
	{
		nextPageButton.Disabled = pageIndex == maxPageIndex;
		previousPageButton.Disabled = pageIndex == 0;
	}

	private void OnPageChanged(int changed)
	{
		pageIndex += changed;
		ShowPage();
	}
}
