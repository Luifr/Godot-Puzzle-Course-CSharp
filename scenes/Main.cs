using Godot;
using System;

namespace Game;

public partial class Main : Node2D
{
  private Sprite2D sprite;
  private PackedScene buildinScene;
  private Button placeBuildingButton;


  public override void _Ready()
  {
    buildinScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
    sprite = GetNode<Sprite2D>("Sprite2D");
    placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("left_click"))
    {
      PlaceBuildingAtMousePosition();
    }
  }

  public void PlaceBuildingAtMousePosition()
  {
    var building = buildinScene.Instantiate<Node2D>();
    building.GlobalPosition = GetMouseGridCellPosition() * 64;

    AddChild(building);
  }

  private Vector2 GetMouseGridCellPosition()
  {
    var mousePosition = GetGlobalMousePosition();
    return (mousePosition / 64).Floor();
  }

  public override void _Process(double delta)
  {
    sprite.GlobalPosition = GetMouseGridCellPosition() * 64;
  }
}
