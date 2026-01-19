using Game.Manager;
using Game.UI;
using Godot;

namespace Game;

public partial class BaseLevel : Node
{

  #region Node references
  private GridManager gridManager;
  private GoldMine goldMine;
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

    gridManager.GridStateUpdated += OnGridStateUpdated;
  }

  public override void _Process(double _delta)
  {
    // TODO: remove
    gameUI.UpdateWoodText(buildingManager.availableResourceCount);
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
