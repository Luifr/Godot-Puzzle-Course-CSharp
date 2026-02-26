using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;
using ExhaustiveMatching;
using System.Linq;
using System.Collections.Generic;

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
	private Rect2I hoveredGridArea = new Rect2I(Vector2I.One, Vector2I.One);
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
					IsBuildingPlaceableAtArea(hoveredGridArea)
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

		if (mouseGridPosition != hoveredGridArea.Position)
		{
			hoveredGridArea.Position = mouseGridPosition;
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

		if (IsBuildingPlaceableAtArea(hoveredGridArea))
		{
			gridManager.HighlightExpandedBuildableTiles(hoveredGridArea, buildingResourceToPlace.buildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridArea, buildingResourceToPlace.resourceRadius);
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
		building.GlobalPosition = hoveredGridArea.Position * GridManager.TILE_SIZE;

		ySortRoot.AddChild(building);

		currentlyUsedResourcesCount += buildingResourceToPlace.resourceCost;

		ClearBuildingGhost();

		ChangeState(State.Normal);
	}

	private void DestroyBuildingAtHoveredCellPosition()
	{
		var rootCell = hoveredGridArea.Position;
		var hoveredBuildingComponent =
			gridManager.GetAllBuildingComponents()
			.FirstOrDefault((buildingComponent) => buildingComponent.IsTileInBuildingArea(rootCell));

		if (hoveredBuildingComponent == null) return;
		if (!hoveredBuildingComponent.buildingResource.isDeletable) return;

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

	private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
	{
		var positions =GetTilePositionsInTileArea(tileArea);

		return
			positions.All(gridManager.IsTilePositionBuildable) &&
			availableResourceCount >= buildingResourceToPlace.resourceCost;
	}

	private List<Vector2I> GetTilePositionsInTileArea(Rect2I tileArea)
	{
		var result = new List<Vector2I>();

		for (int x = tileArea.Position.X; x < tileArea.End.X; x += 1)
		{
			for (int y = tileArea.Position.Y; y < tileArea.End.Y; y += 1)
			{
				result.Add(new Vector2I(x, y));
			}
		}

		return result;
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

		hoveredGridArea.Size = buildingResource.dimensions;
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
