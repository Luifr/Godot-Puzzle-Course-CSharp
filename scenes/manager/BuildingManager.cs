using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;
using ExhaustiveMatching;
using Game.Component;
using System.Linq;

namespace Game.Manager;

public partial class BuildingManager : Node
{

	private enum State
	{
		Normal,
		PlacingBuilding
	}

	#region Export node references
	[Export]
	private GridManager gridManager;
	[Export]
	private GameUI gameUI;
	[Export]
	private Node2D ySortRoot;
	[Export]
	private PackedScene buildingGhostScene;
	#endregion

	#region Private members
	[Export]
	private int startingResourceCount = 4;
	private int currentResourcesCount;
	private int currentlyUsedResourcesCount;
	private Vector2I hoveredGridCell;
	private BuildingResource buildingResourceToPlace;
	private BuildingGhost buildingGhost;
	private State currentState = State.Normal;

	// TODO: change back to private
	public int availableResourceCount => startingResourceCount + currentResourcesCount - currentlyUsedResourcesCount;
	#endregion

	#region Constants
	private readonly StringName ACTION_LEFT_CLICK = "left_click";
	private readonly StringName ACTION_RIGHT_CLICK = "right_click";
	private readonly StringName ACTION_CANCEL = "cancel";
	#endregion

	public override void _Ready()
	{
		gameUI.PlaceBuildingButtonPressed += OnPlaceBuildingButtonPressed;
		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		switch (currentState)
		{
			case State.Normal:
				if (@event.IsActionPressed(ACTION_RIGHT_CLICK))
				{
					DestroyBuildingAtHoveredCellPosition();
				}
				break;
			case State.PlacingBuilding:
				if (@event.IsActionPressed(ACTION_CANCEL))
				{
					ClearBuildingGhost();
					ChangeState(State.Normal);
				}
				else if (
					@event.IsActionPressed(ACTION_LEFT_CLICK) &&
					IsBuildingPlaceableAtTile(hoveredGridCell)
				)
				{
					PlaceBuildingAtHoveredCellPosition();
				}
				break;
			default:
				throw ExhaustiveMatch.Failed(currentState);
		}
	}

	public override void _Process(double delta)
	{

		var mouseGridPosition = gridManager.GetMouseGridCellPosition();

		if (mouseGridPosition != hoveredGridCell)
		{
			hoveredGridCell = mouseGridPosition;
			UpdateHoveredGridCell();
		}

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost.GlobalPosition = mouseGridPosition * GridManager.TILE_SIZE;
				break;
			default:
				throw ExhaustiveMatch.Failed(currentState);
		}
	}

	private void UpdateGridDisplay()
	{
		gridManager.ClearHighlightedTiles();
		gridManager.HighlightBuildableTiles();

		if (IsBuildingPlaceableAtTile(hoveredGridCell))
		{
			gridManager.HighlightExpandedBuildableTiles(hoveredGridCell, buildingResourceToPlace.buildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridCell, buildingResourceToPlace.resourceRadius);
			buildingGhost.SetValid();
		}
		else
		{
			buildingGhost.SetInvalid();
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		var building = buildingResourceToPlace.buildableScene.Instantiate<Node2D>();
		building.GlobalPosition = hoveredGridCell * GridManager.TILE_SIZE;

		ySortRoot.AddChild(building);

		currentlyUsedResourcesCount += buildingResourceToPlace.resourceCost;

		ClearBuildingGhost();

		ChangeState(State.Normal);
	}

	private void DestroyBuildingAtHoveredCellPosition()
	{
		var hoveredBuildingComponent =
			gridManager.GetAllBuildingComponents()
			.FirstOrDefault((buildingComponent) => buildingComponent.GetGridCellPosition() == hoveredGridCell);

		if (hoveredBuildingComponent == null) return;

		currentlyUsedResourcesCount -= hoveredBuildingComponent.buildingResource.resourceCost;
		hoveredBuildingComponent.Destroy();

	}

	private void ClearBuildingGhost()
	{
		if (IsInstanceValid(buildingGhost))
		{
			buildingGhost.QueueFree();
		}
		buildingGhost = null;

		gridManager.ClearHighlightedTiles();
	}

	private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
	{
		return
			gridManager.IsTilePositionBuildable(tilePosition) &&
			availableResourceCount >= buildingResourceToPlace.resourceCost;
	}

	private void UpdateHoveredGridCell()
	{
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				UpdateGridDisplay();
				break;
			default: throw ExhaustiveMatch.Failed(currentState);
		}
	}

	private void ChangeState(State toState)
	{
		// Exiting state handle
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				ClearBuildingGhost();
				buildingResourceToPlace = null;
				break;
			default:
				throw ExhaustiveMatch.Failed(currentState);
		}

		currentState = toState;

		// Entering state handle
		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				break;
			default:
				throw ExhaustiveMatch.Failed(currentState);
		}
	}

	private void OnPlaceBuildingButtonPressed(BuildingResource buildingResource)
	{
		ChangeState(State.PlacingBuilding);

		buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();

		ySortRoot.AddChild(buildingGhost);

		var buildingSprite = buildingResource.spriteScene.Instantiate<Sprite2D>();
		buildingGhost.AddChild(buildingSprite);

		buildingResourceToPlace = buildingResource;
		UpdateGridDisplay();
	}

	private void OnResourceTilesUpdated(int resourceCount)
	{
		currentResourcesCount = resourceCount;
	}
}
