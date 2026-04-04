using Game.AutoLoad;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{

	#region Export references
	[Export]
	private BuildingResource[] buildingResources;
	[Export]
	private PackedScene buildingSectionScene;
	#endregion

	#region Node references
	private VBoxContainer buildingSectionContainer;
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
		woodLabel = GetNode<Label>("%WoodLabel");

		buildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");

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
			var buildingSection = buildingSectionScene.Instantiate<BuildingSection>();

			buildingSection.SelectButtonPressed += () => EmitSignalPlaceBuildingButtonPressed(buildingResource);

			buildingSectionContainer.AddChild(buildingSection);

			buildingSection.SetBuildingResource(buildingResource);
		}
	}
}
