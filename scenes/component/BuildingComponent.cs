using Game.AutoLoad;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{

  #region Public Members
  [Export]
  public int buildableRadius { get; private set; }
  #endregion

  public override void _Ready()
  {
    AddToGroup(nameof(BuildingComponent));
    Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
  }

  public Vector2I GetGridCellPosition()
  {
    var gridPosition = (GlobalPosition / 64).Floor();

    return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
  }
}
