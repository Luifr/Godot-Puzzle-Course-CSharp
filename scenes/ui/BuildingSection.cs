using Game.AutoLoad;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class BuildingSection : PanelContainer
{

	private Label titleLabel;
	private Label descriptionsLabel;
	private Label costLabel;
	private Button selectButton;

	[Signal] public delegate void SelectButtonPressedEventHandler();

	public override void _Ready()
	{
		titleLabel = GetNode<Label>("%TitleLabel");
		descriptionsLabel = GetNode<Label>("%DescriptionLabel");
		costLabel = GetNode<Label>("%CostLabel");
		selectButton = GetNode<Button>("%Button");

		AudioHelpers.RegisterButtons([selectButton]);

		selectButton.Pressed += EmitSignalSelectButtonPressed;
	}

	public void SetBuildingResource(BuildingResource buildingResource)
	{
		titleLabel.Text = buildingResource.displayName;
		costLabel.Text = buildingResource.resourceCost.ToString();
		descriptionsLabel.Text = buildingResource.description;
	}
}
