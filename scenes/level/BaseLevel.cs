using Game.AutoLoad;
using Game.Component;
using Game.Manager;
using Game.UI;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{

  #region Node references
  private GridManager gridManager;
  private GoldMine goldMine;
  private GameCamera gameCamera;
  private TileMapLayer baseTerrainTileMapLayer;
  private Node2D baseBuilding;
  #endregion

  // TODOT: TEMP remove
  private GameUI gameUI;
  private BuildingManager buildingManager;

  public override void _Ready()
  {
    // TOOO: temp remove
    gameUI = GetNode<GameUI>("GameUI");
    buildingManager = GetNode<BuildingManager>("BuildingManager");
    //

    gridManager = GetNode<GridManager>("GridManager");
    goldMine = GetNode<GoldMine>("%GoldMine");
    gameCamera = GetNode<GameCamera>("GameCamera");
    baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
    baseBuilding = GetNode<Node2D>("%Base");

    gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
    gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

    gridManager.GridStateUpdated += OnGridStateUpdated;

    // TODO: remove
    GameEvents.Instance.BuildingPlaced += (_) => gameUI.UpdateWoodText(buildingManager.availableResourceCount);
    GameEvents.Instance.BuildingDestroyed += (_, _) => gameUI.UpdateWoodText(buildingManager.availableResourceCount);
  }

  private void OnGridStateUpdated()
  {
    var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
    if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
    {
      GD.Print("Win");
      goldMine.SetActive();
    }
  }
}
