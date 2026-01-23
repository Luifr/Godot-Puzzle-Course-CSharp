using System;
using System.Collections.Generic;
using System.Linq;
using Game.AutoLoad;
using Game.Component;
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
	private HashSet<Vector2I> validBuildableTiles = new();
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
	}

	public bool TileHasCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTileMapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);

			if (customData == null || (bool)customData.GetCustomData(IS_IGNORED)) continue;

			return (bool)customData.GetCustomData(dataName);
		}

		return false;
	}

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in validBuildableTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
	{
		var validTiles = GetValidTilesInRadius(rootCell, radius);
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());

		var atlasCoord = new Vector2I(1, 0);

		foreach (var expandedTile in expandedTiles)
		{
			highlightTileMapLayer.SetCell(expandedTile, 0, atlasCoord);
		}
	}

	public void HighlightResourceTiles(Vector2I rootCell, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(rootCell, radius);

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

	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.buildingResource.buildableRadius);

		validBuildableTiles.UnionWith(validTiles);

		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var oldResourceCount = collectedresourceTiles.Count;

		var rootCell = buildingComponent.GetGridCellPosition();
		var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.buildingResource.resourceRadius);
		collectedresourceTiles.UnionWith(resourceTiles);

		if (collectedresourceTiles.Count != oldResourceCount)
			EmitSignalResourceTilesUpdated(collectedresourceTiles.Count);
	}

	private void RecalculateValidTiles()
	{
		validBuildableTiles.Clear();
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

	private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
	{
		List<Vector2I> list = new();

		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I((int)x, (int)y);
				if (!filterFn(tilePosition)) continue;
				list.Add(tilePosition);
			}
		}

		return list;
	}

	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(
			rootCell,
			radius,
			(tilePosition) => TileHasCustomData(tilePosition, IS_BUILDABLE)
		);
	}

	private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(
			rootCell,
			radius,
			(tilePosition) => TileHasCustomData(tilePosition, IS_WOOD)
		);
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponents = GetAllBuildingComponents();
		var occupiedPositions = buildingComponents.Select((x) => x.GetGridCellPosition());
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
		if (buildingResource.buildableRadius > 0)
			RecalculateValidTiles();
		if (buildingResource.resourceRadius > 0)
			RecalculateCollectedResourceTiles();
	}
}
