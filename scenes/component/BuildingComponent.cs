using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Game.AutoLoad;
using Game.Manager;
using Game.Resources.Building;
using Godot;

namespace Game.Component;

public partial class BuildingComponent : Node2D
{
  [Signal] public delegate void EnabledEventHandler();
  [Signal] public delegate void DisabledEventHandler();

  [Export(PropertyHint.File, "*.tres")]
  private string buildingResourcePath;

  private HashSet<Vector2I> occupiedTiles = new();

  #region Public Members
  public BuildingResource buildingResource { get; private set; }
  public bool isDisabled { get; private set; }
  #endregion

  public override void _Ready()
  {
    if (buildingResourcePath != null)
    {
      buildingResource = GD.Load<BuildingResource>(buildingResourcePath);
    }

    AddToGroup(nameof(BuildingComponent));
    Callable.From(Initialize).CallDeferred();
  }

  public Vector2I GetGridCellPosition()
  {
    var gridPosition = (GlobalPosition / GridManager.TILE_SIZE).Floor();

    return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
  }

  public HashSet<Vector2I> GetOccupiedCellPositions()
  {
    return occupiedTiles.ToHashSet();
  }

  public Rect2I GetTileArea()
  {
    var rootCell = GetGridCellPosition();
    return new Rect2I(rootCell, buildingResource.dimensions);
  }

  public bool IsTileInBuildingArea(Vector2I tilePosition)
  {
    return occupiedTiles.Contains(tilePosition);
  }

  public bool IsPlayingAnimation()
  {
    var buildingAnimatorComponent = Owner.GetFirstNodeOfType<BuildingAnimatorComponent>();
    if (buildingAnimatorComponent == null) return false;

    return buildingAnimatorComponent.IsPlayingAnimation();
  }

  public void Destroy()
  {
    if (Owner == null) return;

    Owner.TreeExited += () => GameEvents.EmitBuildingDestroyed(buildingResource, (Vector2I)GlobalPosition);
    var buildingAnimatorComponent = Owner.GetFirstNodeOfType<BuildingAnimatorComponent>();
    buildingAnimatorComponent?.PlayDestroyAnimation();

    if (buildingAnimatorComponent == null)
      Owner.QueueFree();
    else
      buildingAnimatorComponent.DestroyAnimationFinished += Owner.QueueFree;
  }

  public void Enable()
  {
    if (!isDisabled) return;

    isDisabled = false;
    EmitSignalEnabled();
    GameEvents.EmitBuildingEnabled(this);
  }

  public void Disable()
  {
    if (isDisabled) return;

    isDisabled = true;
    EmitSignalDisabled();
    GameEvents.EmitBuildingDisabled(this);
  }

  private void CalculateOccupiedCellPositions()
  {
    occupiedTiles.Clear();
    var gridCellPosition = GetGridCellPosition();

    for (int x = gridCellPosition.X; x < gridCellPosition.X + buildingResource.dimensions.X; x += 1)
    {
      for (int y = gridCellPosition.Y; y < gridCellPosition.Y + buildingResource.dimensions.Y; y += 1)
      {
        occupiedTiles.Add(new Vector2I(x, y));
      }

    }
  }

  private void Initialize()
  {
    CalculateOccupiedCellPositions();
    GameEvents.EmitBuildingPlaced(this);
  }
}
