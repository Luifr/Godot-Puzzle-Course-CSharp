using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{

	private Label titleLabel;
	private Button selectButton;

	[Signal] public delegate void SelectButtonPressedEventHandler();

	public override void _Ready()
	{
		titleLabel = GetNode<Label>("%Label");
		selectButton = GetNode<Button>("%Button");

		selectButton.Pressed += EmitSignalSelectButtonPressed;
	}

	public void SetBuildingResource(BuildingResource buildingResource)
	{
		titleLabel.Text = buildingResource.displayName;
		selectButton.Text = $"Select (Cost {buildingResource.resourceCost})";
	}
}
