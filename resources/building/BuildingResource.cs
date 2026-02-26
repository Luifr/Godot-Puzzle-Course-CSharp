using Godot;

namespace Game.Resources.Building;

[GlobalClass]
public partial class BuildingResource : Resource
{
	[Export]
	public string displayName { get; private set; }
	[Export]
	public bool isDeletable { get; private set; } = true;
	[Export]
	public Vector2I dimensions { get; private set; } = Vector2I.One;
	[Export]
	public int resourceCost { get; private set; }
	[Export]
	public int buildableRadius { get; private set; }
	[Export]
	public int resourceRadius { get; private set; }
	[Export]
	public PackedScene buildableScene { get; private set; }
	[Export]
	public PackedScene spriteScene { get; private set; }
}
