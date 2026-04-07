using Game.Resources.Level;
using Godot;

namespace Game.UI;

public partial class LevelSelectSection : PanelContainer
{
	[Signal] public delegate void LevelSelectedEventHandler(int levelIndex);

	private Label levelNumberLabel;
	private Label resourceCountLabel;
	private Button button;

	private int levelIndex;

	public override void _Ready()
	{
		levelNumberLabel = GetNode<Label>("%LevelNumberLabel");
		resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
		button = GetNode<Button>("%Button");

		button.Pressed += () => EmitSignalLevelSelected(levelIndex);
	}

	public void SetLevelDefinition(LevelDefinitionResource levelDefinitionResource)
	{
		resourceCountLabel.Text = levelDefinitionResource.startingResourceCount.ToString();
	}

	public void SetLevelIndex(int index)
	{
		levelIndex = index;
		levelNumberLabel.Text = $"Level {index + 1}";
	}
}
