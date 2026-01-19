using Game.AutoLoad;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{

  [Export(PropertyHint.File, "*.tres")]
  public string buildingResourcePath;

  #region Public Members
  public BuildingResource buildingResource { get; private set; }
  #endregion

  public override void _Ready()
  {
    if (buildingResourcePath != null)
    {
      buildingResource = GD.Load<BuildingResource>(buildingResourcePath);
    }

    AddToGroup(nameof(BuildingComponent));
    Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
  }

  public Vector2I GetGridCellPosition()
  {
    var gridPosition = (GlobalPosition / GridManager.TILE_SIZE).Floor();

    return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
  }

  public void Destroy()
  {
    if (Owner == null) return;

    Owner.TreeExited += () => GameEvents.EmitBuildingDestroyed(buildingResource, (Vector2I)GlobalPosition);
    Owner.QueueFree();
  }
}
