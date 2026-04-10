using Game.Component;
using Game.Resources.Building;
using Godot;

namespace Game.AutoLoad;

public partial class GameEvents : Node
{
	public static GameEvents Instance { get; private set; }

	#region Signals
	[Signal]
	public delegate void BuildingPlacedEventHandler(BuildingComponent buildingComponent);
	[Signal]
	public delegate void BuildingEnabledEventHandler(BuildingComponent buildingComponent);
	[Signal]
	public delegate void BuildingDisabledEventHandler(BuildingComponent buildingComponent);
	[Signal]
	public delegate void BuildingDestroyedEventHandler(BuildingResource buildingResource, Vector2I buildingPosition);
	#endregion

	public override void _EnterTree()
	{
		Instance = this;
	}

	public static void EmitBuildingPlaced(BuildingComponent buildingComponent)
	{
		Instance.EmitSignalBuildingPlaced(buildingComponent);
	}

	public static void EmitBuildingEnabled(BuildingComponent buildingComponent)
	{
		Instance.EmitSignalBuildingEnabled(buildingComponent);
	}

	public static void EmitBuildingDisabled(BuildingComponent buildingComponent)
	{
		Instance.EmitSignalBuildingDisabled(buildingComponent);
	}

	public static void EmitBuildingDestroyed(BuildingResource buildingResource, Vector2I buildingPosition)
	{
		Instance.EmitSignalBuildingDestroyed(buildingResource, buildingPosition);
	}
}
