using Game.Building;
using Game.Resources.Building;
using Game.UI;
using Godot;
using ExhaustiveMatching;
using System.Linq;
using System;

namespace Game.Manager;

public partial class BuildingManager : Node
{

	private struct BuildingResources
	{
		public BuildingResources(int startingResourceCount, Action<int> AvailableResourceCountChanged)
		{
			StartingResourceCount = startingResourceCount;
			_availableResourceCountChanged = AvailableResourceCountChanged;
		}

		private Action<int> _availableResourceCountChanged;

		public int StartingResourceCount { get; set; }
		private int _currentResourceCount;
		public int CurrentResourceCount
		{
			get => _currentResourceCount;
			set
			{
				_currentResourceCount = value;
				_availableResourceCountChanged(AvailableResourceCount);
			}
		}
		private int _currentlyUsedResourceCount;
		public int CurrentlyUsedResourceCount
		{
			get => _currentlyUsedResourceCount;
			set
			{
				_currentlyUsedResourceCount = value;
				_availableResourceCountChanged(AvailableResourceCount);
			}
		}
		public int AvailableResourceCount => StartingResourceCount + CurrentResourceCount - CurrentlyUsedResourceCount;
	}

	private enum State
	{
		Normal,
		PlacingBuilding
	}

	[Signal] public delegate void AvailableResourceCountChangedEventHandler(int newResourceCount);

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
	private Rect2I hoveredGridArea = new Rect2I(Vector2I.One, Vector2I.One);
	private BuildingResource buildingResourceToPlace;
	private BuildingGhost buildingGhost;
	private State currentState = State.Normal;
	private BuildingResources resources;
	#endregion

	#region Constants
	private readonly StringName ACTION_LEFT_CLICK = "left_click";
	private readonly StringName ACTION_RIGHT_CLICK = "right_click";
	private readonly StringName ACTION_CANCEL = "cancel";
	#endregion

	public override void _Ready()
	{
		resources = new BuildingResources(0, EmitSignalAvailableResourceCountChanged);

		gameUI.PlaceBuildingButtonPressed += OnPlaceBuildingButtonPressed;
		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

		Callable.From(() => EmitSignalAvailableResourceCountChanged(resources.AvailableResourceCount)).CallDeferred();
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

		switch (currentState)
		{
			case State.Normal:
				break;
			case State.PlacingBuilding:
				buildingGhost.GlobalPosition = mouseGridPosition * GridManager.TILE_SIZE;
				break;
			default:
				// TODO: Check if there is a native c# way to do this
				throw ExhaustiveMatch.Failed(currentState);
		}

		if (mouseGridPosition != hoveredGridArea.Position)
		{
			hoveredGridArea.Position = mouseGridPosition;
			UpdateHoveredGridCell();
		}
	}

	public void SetStartingResourceCount(int count)
	{
		resources.StartingResourceCount = count;
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

		buildingGhost.DoHoverAnimation();
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		var building = buildingResourceToPlace.buildableScene.Instantiate<Node2D>();
		building.GlobalPosition = hoveredGridArea.Position * GridManager.TILE_SIZE;

		ySortRoot.AddChild(building);

		resources.CurrentlyUsedResourceCount += buildingResourceToPlace.resourceCost;

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

		resources.CurrentlyUsedResourceCount -= hoveredBuildingComponent.buildingResource.resourceCost;
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
		if (resources.AvailableResourceCount < buildingResourceToPlace.resourceCost)
			return false;

		return gridManager.IsTileAreaBuildable(tileArea);
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
		buildingGhost.AddSpriteNode(buildingSprite);
		buildingGhost.SetDimensions(buildingResource.dimensions);


		buildingResourceToPlace = buildingResource;
		UpdateGridDisplay();
	}

	private void OnResourceTilesUpdated(int resourceCount)
	{
		resources.CurrentResourceCount = resourceCount;
	}
}
