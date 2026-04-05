using Game.AutoLoad;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{

	#region Export references
	[Export]
	private BuildingManager buildingManager;
	[Export]
	private BuildingResource[] buildingResources;
	[Export]
	private PackedScene buildingSectionScene;
	#endregion

	#region Node references
	private VBoxContainer buildingSectionContainer;
	private Label resourceLabel;
	#endregion

	#region Signals
	[Signal]
	public delegate void PlaceBuildingButtonPressedEventHandler(BuildingResource buildingResource);
	#endregion


	public override void _Ready()
	{

		buildingSectionContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");
		resourceLabel = GetNode<Label>("%ResourceLabel");

		buildingManager.AvailableResourceCountChanged += OnAvailableResourceCountChanged;

		CreateBuildingButtons();
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

	private void OnAvailableResourceCountChanged(int newResourceCount)
	{
		resourceLabel.Text = newResourceCount.ToString();
	}
}
