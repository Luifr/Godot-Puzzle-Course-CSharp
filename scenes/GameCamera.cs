using Game.Manager;
using Godot;

namespace Game;

public partial class GameCamera : Camera2D
{

	private readonly StringName ACTION_PAN_LEFT = "pan_left";
	private readonly StringName ACTION_PAN_RIGHT = "pan_right";
	private readonly StringName ACTION_PAN_UP = "pan_up";
	private readonly StringName ACTION_PAN_DOWN = "pan_down";

	private readonly float PAN_SPEED = 550;

	public override void _Process(double delta)
	{
		var movementVector = Input.GetVector(
			ACTION_PAN_LEFT, ACTION_PAN_RIGHT, ACTION_PAN_UP, ACTION_PAN_DOWN
		);

		GlobalPosition = GetScreenCenterPosition();
		GlobalPosition += movementVector * PAN_SPEED * (float)delta;
	}

	public void CenterOnPosition(Vector2 position)
	{
		GlobalPosition = position;
	}

	public void SetBoundingRect(Rect2I boudingRect)
	{
		LimitSmoothed = true;
		LimitEnabled = true;

		LimitLeft = boudingRect.Position.X * GridManager.TILE_SIZE;
		LimitRight = boudingRect.End.X * GridManager.TILE_SIZE;

		LimitTop = boudingRect.Position.Y * GridManager.TILE_SIZE;
		LimitBottom = boudingRect.End.Y * GridManager.TILE_SIZE;
	}
}
