using Game.AutoLoad;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : MarginContainer
{

	#region Export references
	[Export]
	private BuildingResource[] buildingResources;
	#endregion

	#region Node references
	private HBoxContainer hBoxContainer;
	#endregion

	#region Signals
	[Signal]
	public delegate void PlaceBuildingButtonPressedEventHandler(BuildingResource buildingResource);
	#endregion


	// TEMP
	Label woodLabel;

	public override void _Ready()
	{
		// TEMP
		woodLabel = GetNode<Label>("WoodLabel");

		hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");

		CreateBuildingButtons();

	}

	// TEMP
	public void UpdateWoodText(int availableWook)
	{
		woodLabel.Text = availableWook.ToString();
	}

	private void CreateBuildingButtons()
	{
		foreach (var buildingResource in buildingResources)
		{
			var buildingButton = new Button();
			buildingButton.Text = $"Place {buildingResource.displayName}";

			buildingButton.Pressed += () => EmitSignalPlaceBuildingButtonPressed(buildingResource);

			hBoxContainer.AddChild(buildingButton);
		}
	}
}
