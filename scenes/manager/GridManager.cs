using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Game.AutoLoad;
using Game.Component;
using Game.Level.Util;
using Game.Resources.Building;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{
	#region Node References
	[Export]
	private TileMapLayer highlightTileMapLayer;
	[Export]
	private TileMapLayer baseTerrainTileMapLayer;
	#endregion

	#region Private Members
	private List<TileMapLayer> allTileMapLayers = new();
	private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = new();
	private Dictionary<BuildingComponent, HashSet<Vector2I>> buildingToBuildable = new();
	private HashSet<Vector2I> validBuildableTiles = new();
	private HashSet<Vector2I> validBuildableAttackTiles = new();
	private HashSet<Vector2I> goblinOccupiedTiles = new();
	private HashSet<Vector2I> attackTiles = new();
	private HashSet<Vector2I> allTilesInBuildingRadius = new();
	private HashSet<Vector2I> collectedresourceTiles = new();
	#endregion

	#region Constants
	private const string IS_BUILDABLE = "is_buildable";
	private const string IS_WOOD = "is_wood";
	private const string IS_IGNORED = "is_ignored";
	public const int TILE_SIZE = 64;
	#endregion

	#region Signals
	[Signal]
	public delegate void ResourceTilesUpdatedEventHandler(int count);
	[Signal]
	public delegate void GridStateUpdatedEventHandler();
	#endregion

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
		GameEvents.Instance.BuildingEnabled += OnBuildingEnabled;
		GameEvents.Instance.BuildingDisabled += OnBuildingDisabled;

		allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
		MapTileMapLayersToElevationLayers();
	}

	public override void _ExitTree()
	{
		GameEvents.Instance.BuildingPlaced -= OnBuildingPlaced;
		GameEvents.Instance.BuildingDestroyed -= OnBuildingDestroyed;
		GameEvents.Instance.BuildingEnabled += OnBuildingEnabled;
		GameEvents.Instance.BuildingDisabled += OnBuildingDisabled;
	}

	public (TileMapLayer, bool) GetTileCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTileMapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null || (bool)customData.GetCustomData(IS_IGNORED)) continue;

			return (layer, (bool)customData.GetCustomData(dataName));
		}

		return (null, false);
	}

	public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
	{
		return allTilesInBuildingRadius.Contains(tilePosition);
	}

	public bool IsTileAreaBuildable(Rect2I tileArea, bool isAttackTiles = false)
	{
		var tiles = tileArea.ToTiles();

		if (tiles.Count == 0) return false;

		(TileMapLayer firstTileMapLayer, bool isFirstTileBuildable) = GetTileCustomData(tiles[0], IS_BUILDABLE);
		if (!isFirstTileBuildable) return false;

		var targetElevationLayer = tileMapLayerToElevationLayer[firstTileMapLayer];

		var tileSetToCheck = GetBuildableTileSet(isAttackTiles);
		if (isAttackTiles)
		{
			tileSetToCheck = tileSetToCheck.Except(GetOccupiedTiles()).ToHashSet();
		}

		return tiles.All((tile) =>
		{
			(TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(tile, IS_BUILDABLE);
			var elevationLayer = tileMapLayerToElevationLayer[tileMapLayer];

			return isBuildable && tileSetToCheck.Contains(tile) && elevationLayer == targetElevationLayer;
		});
	}

	public void HighlightGoblinOccupiedTiles()
	{
		var atlasCoord = new Vector2I(2, 0);
		foreach (var tilePosition in goblinOccupiedTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoord);
		}
	}

	public void HighlightBuildableTiles(bool isAttackTiles = false)
	{
		foreach (var tilePosition in GetBuildableTileSet(isAttackTiles))
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
	{
		var validTiles = GetValidTilesInRadius(tileArea, radius);
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());

		var atlasCoord = new Vector2I(1, 0);

		foreach (var tilePosition in expandedTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoord);
		}
	}

	public void HighlightBuildableAttackTiles(Rect2I tileArea, int radius)
	{
		var validTiles = GetValidTilesInRadius(tileArea, radius)
			.ToHashSet()
			.Except(validBuildableAttackTiles)
			.Except(GetOccupiedTiles())
			.Except(tileArea.ToTiles());

		var atlasCoord = new Vector2I(1, 0);

		foreach (var tilePosition in validTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoord);
		}
	}

	public void HighlightResourceTiles(Rect2I tileArea, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(tileArea, radius);

		var atlasCoord = new Vector2I(1, 0);

		foreach (var resourceTile in resourceTiles)
		{
			highlightTileMapLayer.SetCell(resourceTile, 0, atlasCoord);
		}

	}

	public void ClearHighlightedTiles()
	{
		highlightTileMapLayer.Clear();
	}

	public Vector2I GetMouseGridCellPositionWithDimensionOffset(Vector2 dimensions)
	{
		var mousePosition = highlightTileMapLayer.GetGlobalMousePosition() / TILE_SIZE;
		mousePosition -= dimensions / 2;
		mousePosition = mousePosition.Round();

		return (Vector2I)mousePosition;
	}

	public Vector2I GetMouseGridCellPosition()
	{
		var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
		return ConvertWorldPositionToTilePosition(mousePosition);
	}

	public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
	{
		var tilePosition = (worldPosition / TILE_SIZE).Floor();

		return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
	}

	public bool CanDestroyBuilding(BuildingComponent toDestroyBuildingComponent)
	{
		if (toDestroyBuildingComponent.buildingResource.buildableRadius > 0)
		{
			var dependentBuildings = GetAllBuildingComponents()
				.Where(buildingComponent =>
				{
					var anyTilesInRadius = buildingComponent.GetTileArea().ToTiles()
						.Any(tilePosition => buildingToBuildable[toDestroyBuildingComponent].Contains(tilePosition));

					return buildingComponent != toDestroyBuildingComponent && anyTilesInRadius;
				});

			var allBuildingsStillValid = dependentBuildings.All(dependentBuilding =>
			{
				var tilesForBuilding = dependentBuilding.GetTileArea().ToTiles();
				return tilesForBuilding.All(tilePosition =>
				{
					var tileIsInSet = buildingToBuildable.Keys
						.Any(buildingComponent => buildingComponent != toDestroyBuildingComponent && buildingToBuildable[buildingComponent].Contains(tilePosition));

					return tileIsInSet;
				});
			});

			return allBuildingsStillValid;
		}

		return true;
	}

	public IEnumerable<BuildingComponent> GetAllBuildingComponents()
	{
		return GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
	}

	private HashSet<Vector2I> GetBuildableTileSet(bool isAttackTiles = false)
	{
		return isAttackTiles ? validBuildableAttackTiles : validBuildableTiles;
	}

	private List<TileMapLayer> GetAllTileMapLayers(Node node)
	{
		var allTileMapLayers = new List<TileMapLayer>();

		var children = node.GetChildren();
		children.Reverse();

		foreach (var child in children)
		{
			if (child is Node childNode)
			{
				allTileMapLayers.AddRange(GetAllTileMapLayers(childNode));
			}
		}

		if (node is TileMapLayer tileMapLayer)
		{
			allTileMapLayers.Add(tileMapLayer);
		}

		return allTileMapLayers;
	}

	private void MapTileMapLayersToElevationLayers()
	{
		foreach (var layer in allTileMapLayers)
		{
			ElevationLayer elevationLayer;
			Node currentNode = layer;

			do
			{
				var parent = currentNode.GetParent();
				elevationLayer = parent as ElevationLayer;
				currentNode = parent;
			} while (elevationLayer == null && currentNode != null);

			tileMapLayerToElevationLayer[layer] = elevationLayer;
		}
	}

	private void UpdateGoblinOccupiedTiles(BuildingComponent buildingComponent)
	{
		if (!buildingComponent.buildingResource.IsDangerBuilding || buildingComponent.isDisabled) return;

		var tileArea = buildingComponent.GetTileArea();
		var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.buildingResource.dangerRadius);
		goblinOccupiedTiles.UnionWith(validTiles);
		goblinOccupiedTiles.ExceptWith(GetOccupiedTiles());
		goblinOccupiedTiles.ExceptWith(tileArea.ToTiles());
	}

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var tileArea = buildingComponent.GetTileArea();

		if (buildingComponent.buildingResource.buildableRadius > 0)
		{
			var allTiles = GetTilesInRadius(tileArea, buildingComponent.buildingResource.buildableRadius, (_) => true);
			allTilesInBuildingRadius.UnionWith(allTiles);

			var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.buildingResource.buildableRadius);
			buildingToBuildable[buildingComponent] = validTiles.ToHashSet();
			validBuildableTiles.UnionWith(validTiles);
		}

		validBuildableTiles.ExceptWith(GetOccupiedTiles());
		validBuildableAttackTiles.UnionWith(validBuildableTiles);

		validBuildableTiles.ExceptWith(goblinOccupiedTiles);
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		if (!buildingComponent.buildingResource.IsResourceBuilding) return;

		var oldResourceCount = collectedresourceTiles.Count;

		var tileArea = buildingComponent.GetTileArea();
		var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.buildingResource.resourceRadius);
		collectedresourceTiles.UnionWith(resourceTiles);

		if (collectedresourceTiles.Count != oldResourceCount)
			EmitSignalResourceTilesUpdated(collectedresourceTiles.Count);
	}

	private void UpdateAttackTiles(BuildingComponent buildingComponent)
	{
		if (!buildingComponent.buildingResource.IsAttackBuilding) return;

		var tileArea = buildingComponent.GetTileArea();
		var newAttackTiles = GetTilesInRadius(tileArea, buildingComponent.buildingResource.attackRadius, (_) => true).ToHashSet();
		attackTiles.UnionWith(newAttackTiles);
	}

	private void RecalculateGrid()
	{
		validBuildableTiles.Clear();
		validBuildableAttackTiles.Clear();
		allTilesInBuildingRadius.Clear();
		goblinOccupiedTiles.Clear();
		attackTiles.Clear();
		buildingToBuildable.Clear();

		var buildingComponents = GetAllBuildingComponents();

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateBuildingComponentGridState(buildingComponent);
		}

		CheckGoblinCampDestruction();

		EmitSignalGridStateUpdated();
	}

	private void CheckGoblinCampDestruction()
	{
		var dangedBuildings = GetAllBuildingComponents()
			.Where(buildingComponent => buildingComponent.buildingResource.IsDangerBuilding);

		foreach (var dangedBuilding in dangedBuildings)
		{
			var tileArea = dangedBuilding.GetTileArea();
			var isInsideAttackTile = tileArea.ToTiles().Any(attackTiles.Contains);
			if (isInsideAttackTile)
			{
				if (!dangedBuilding.isDisabled) dangedBuilding.Disable();
			}
			else
			{
				if (dangedBuilding.isDisabled) dangedBuilding.Enable();
			}
		}
	}

	private bool IsTileInsideCricle(Vector2 centerPosition, Vector2 tilePosition, float radius)
	{
		var distanceX = centerPosition.X - (tilePosition.X + .5);
		var distanceY = centerPosition.Y - (tilePosition.Y + .5);

		var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

		return distanceSquared <= (radius * radius);
	}

	private List<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filterFn)
	{
		List<Vector2I> list = new();
		Vector2 tileAreaCenter = tileArea.ToRect2().GetCenter();
		float radiusMod = Mathf.Max(tileArea.Size.X, tileArea.Size.Y) / 2.0f;


		for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
		{
			for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
			{
				var tilePosition = new Vector2I((int)x, (int)y);
				if (!IsTileInsideCricle(tileAreaCenter, tilePosition, radius + radiusMod) || !filterFn(tilePosition)) continue;
				list.Add(tilePosition);
			}
		}

		return list;
	}

	private List<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(
			tileArea,
			radius,
			(tilePosition) => GetTileCustomData(tilePosition, IS_BUILDABLE).Item2
		);
	}

	private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
	{
		return GetTilesInRadius(
			tileArea,
			radius,
			(tilePosition) => GetTileCustomData(tilePosition, IS_WOOD).Item2
		);
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponents = GetAllBuildingComponents();
		var occupiedPositions = new List<Vector2I>();

		foreach (var buildingComponent in buildingComponents)
		{
			occupiedPositions.AddRange(buildingComponent.GetOccupiedCellPositions());
		}
		return occupiedPositions;
	}

	private void UpdateBuildingComponentGridState(BuildingComponent buildingComponent)
	{
		UpdateGoblinOccupiedTiles(buildingComponent);
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
		UpdateAttackTiles(buildingComponent);

		EmitSignalGridStateUpdated();
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateBuildingComponentGridState(buildingComponent);
		CheckGoblinCampDestruction();
	}

	private void OnBuildingDestroyed(BuildingResource buildingResource, Vector2I buildingPosition)
	{
		RecalculateGrid();
	}

	private void OnBuildingEnabled(BuildingComponent buildingComponent)
	{
		UpdateBuildingComponentGridState(buildingComponent);
	}
	private void OnBuildingDisabled(BuildingComponent buildingComponent)
	{
		RecalculateGrid();
	}
}
