using Game.Component;
using Godot;

namespace Game.AutoLoad;

public partial class GameEvents : Node
{
	public static GameEvents Instance { get; private set; }

	#region Signals
	[Signal]
	public delegate void BuildingPlacedEventHandler(BuildingComponent buildingComponent);
	#endregion

	public override void _EnterTree()
	{
		Instance = this;
	}

	public static void EmitBuildingPlaced(BuildingComponent buildingComponent)
	{
		Instance.EmitSignal(SignalName.BuildingPlaced, buildingComponent);
	}
}
