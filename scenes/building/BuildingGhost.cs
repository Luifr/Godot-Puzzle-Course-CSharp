using Godot;

namespace Game.Building;

public partial class BuildingGhost : Node2D
{
  public void SetInvalid()
  {
    Modulate = Color.FromHtml("#e25e5e");
  }

  public void SetValid()
  {
    Modulate = Colors.White;
  }
}
