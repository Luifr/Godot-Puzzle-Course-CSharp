using Game.AutoLoad;
using Game.Resources.Level;
using Godot;

namespace Game.UI;

public partial class LevelSelectSection : PanelContainer
{
	[Signal] public delegate void LevelSelectedEventHandler(int levelIndex);

	private Label levelNumberLabel;
	private Label resourceCountLabel;
	private TextureRect completedIndicator;
	private Button button;

	private int levelIndex;

	public override void _Ready()
	{
		levelNumberLabel = GetNode<Label>("%LevelNumberLabel");
		resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
		completedIndicator = GetNode<TextureRect>("%CompletedIndicator");
		button = GetNode<Button>("%Button");

		AudioHelpers.RegisterButtons([button]);

		button.Pressed += () => EmitSignalLevelSelected(levelIndex);

	}

	public void SetLevelDefinition(LevelDefinitionResource levelDefinitionResource)
	{
		resourceCountLabel.Text = levelDefinitionResource.startingResourceCount.ToString();
		completedIndicator.Visible = SaveManager.IsLevelCompleted(levelDefinitionResource.Id);
	}

	public void SetLevelIndex(int index)
	{
		levelIndex = index;
		levelNumberLabel.Text = $"Level {index + 1}";
	}
}
