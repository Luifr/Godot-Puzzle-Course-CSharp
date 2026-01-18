using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
  #region Node references
  private GridManager gridManager;
  private Sprite2D cursorSprite;
  private PackedScene buildingScene;
  private Button placeBuildingButton;
  #endregion

  #region Private members
  private Vector2I? hoveredGridCell;
  private int highlightRadius = 3;
  #endregion

  public override void _Ready()
  {
    buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");

    gridManager = GetNode<GridManager>("GridManager");
    cursorSprite = GetNode<Sprite2D>("Cursor");
    placeBuildingButton = GetNode<Button>("PlaceBuildingButton");

    cursorSprite.Hide();

    placeBuildingButton.Pressed += OnPlaceBuildingButtonPressed;
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (
      @event.IsActionPressed("left_click") &&
      hoveredGridCell.HasValue &&
      gridManager.IsTilePositionBuildable(hoveredGridCell.Value)
    )
    {
      PlaceBuildingAtHoveredCellPosition();
    }
  }

  public void PlaceBuildingAtHoveredCellPosition()
  {
    if (!hoveredGridCell.HasValue) return;

    var building = buildingScene.Instantiate<Node2D>();
    building.GlobalPosition = hoveredGridCell.Value * 64;

    AddChild(building);

    cursorSprite.Hide();

    hoveredGridCell = null;
    gridManager.ClearHighlightedTiles();
  }

  public override void _Process(double delta)
  {
    if (cursorSprite.Visible)
    {
      var mouseGridPosition = gridManager.GetMouseGridCellPosition();
      cursorSprite.GlobalPosition = mouseGridPosition * 64;

      if (!hoveredGridCell.HasValue || hoveredGridCell.Value != mouseGridPosition)
      {
        hoveredGridCell = mouseGridPosition;
        gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, 2);
      }
    }
  }

  private void OnPlaceBuildingButtonPressed()
  {
    cursorSprite.Show();
    gridManager.HighlightBuildableTiles();
  }
}
