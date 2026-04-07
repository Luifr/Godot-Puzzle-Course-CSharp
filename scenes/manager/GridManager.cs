using System;
using System.Collections.Generic;
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
	private HashSet<Vector2I> validBuildableTiles = new();
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

		allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
		MapTileMapLayersToElevationLayers();
	}

	public override void _ExitTree()
	{
		GameEvents.Instance.BuildingPlaced -= OnBuildingPlaced;
		GameEvents.Instance.BuildingDestroyed -= OnBuildingDestroyed;
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

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}

	public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
	{
		return allTilesInBuildingRadius.Contains(tilePosition);
	}

	public bool IsTileAreaBuildable(Rect2I tileArea)
	{
		var tiles = tileArea.ToTiles();

		if (tiles.Count == 0) return false;

		(TileMapLayer firstTileMapLayer, bool isFirstTileBuildable) = GetTileCustomData(tiles[0], IS_BUILDABLE);
		if (!isFirstTileBuildable) return false;

		var targetElevationLayer = tileMapLayerToElevationLayer[firstTileMapLayer];

		return tiles.Skip(1).All((tile) =>
		{
			(TileMapLayer tileMapLayer, bool isBuildable) = GetTileCustomData(tile, IS_BUILDABLE);
			var elevationLayer = tileMapLayerToElevationLayer[tileMapLayer];

			return isBuildable && validBuildableTiles.Contains(tile) && elevationLayer == targetElevationLayer;
		});
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in validBuildableTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
	{
		var validTiles = GetValidTilesInRadius(tileArea, radius);
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());

		var atlasCoord = new Vector2I(1, 0);

		foreach (var expandedTile in expandedTiles)
		{
			highlightTileMapLayer.SetCell(expandedTile, 0, atlasCoord);
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

	public IEnumerable<BuildingComponent> GetAllBuildingComponents()
	{
		return GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
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

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, buildingComponent.buildingResource.dimensions);
		var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.buildingResource.buildableRadius);

		var allTiles = GetTilesInRadius(tileArea, buildingComponent.buildingResource.buildableRadius, (_) => true);
		allTilesInBuildingRadius.UnionWith(allTiles);

		validBuildableTiles.UnionWith(validTiles);

		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var oldResourceCount = collectedresourceTiles.Count;

		var rootCell = buildingComponent.GetGridCellPosition();
		var tileArea = new Rect2I(rootCell, buildingComponent.buildingResource.dimensions);
		var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.buildingResource.resourceRadius);
		collectedresourceTiles.UnionWith(resourceTiles);

		if (collectedresourceTiles.Count != oldResourceCount)
			EmitSignalResourceTilesUpdated(collectedresourceTiles.Count);
	}

	private void RecalculateValidTiles()
	{
		validBuildableTiles.Clear();
		allTilesInBuildingRadius.Clear();
		var buildingComponents = GetAllBuildingComponents();

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateValidBuildableTiles(buildingComponent);
		}

		EmitSignalGridStateUpdated();
	}

	private void RecalculateCollectedResourceTiles()
	{
		collectedresourceTiles.Clear();
		var buildingComponents = GetAllBuildingComponents();

		foreach (var buildingComponent in buildingComponents)
		{
			UpdateCollectedResourceTiles(buildingComponent);
		}

		// If there is a single resource building, it gets destroyed, we need to force update the resources
		EmitSignalResourceTilesUpdated(collectedresourceTiles.Count);
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

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);

		EmitSignalGridStateUpdated();
	}

	private void OnBuildingDestroyed(BuildingResource buildingResource, Vector2I buildingPosition)
	{
		RecalculateValidTiles();
		if (buildingResource.resourceRadius > 0)
			RecalculateCollectedResourceTiles();
	}
}
