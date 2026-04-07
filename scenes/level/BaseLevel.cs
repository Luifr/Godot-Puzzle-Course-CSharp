using Game.Manager;
using Game.UI;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{

  [Export]
  private PackedScene LevelCompleteScreenScene;

  #region Node references
  private GridManager gridManager;
  private GoldMine goldMine;
  private GameCamera gameCamera;
  private TileMapLayer baseTerrainTileMapLayer;
  private Node2D baseBuilding;
  private GameUI gameUI;
  #endregion

  public override void _Ready()
  {

    gridManager = GetNode<GridManager>("GridManager");
    gameUI = GetNode<GameUI>("GameUI");
    goldMine = GetNode<GoldMine>("%GoldMine");
    gameCamera = GetNode<GameCamera>("GameCamera");
    baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
    baseBuilding = GetNode<Node2D>("%Base");

    gameCamera.SetBoundingRect(baseTerrainTileMapLayer.GetUsedRect());
    gameCamera.CenterOnPosition(baseBuilding.GlobalPosition);

    gridManager.GridStateUpdated += OnGridStateUpdated;
    gameUI.Show();
  }

  private void OnGridStateUpdated()
  {
    var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
    if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
    {
      goldMine.SetActive();
      var levelCompleteScreenScene = LevelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
      AddChild(levelCompleteScreenScene);
      gameUI.HideUI();
    }
  }
}
