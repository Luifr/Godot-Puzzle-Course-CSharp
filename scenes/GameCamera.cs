using Game.Manager;
using Godot;

namespace Game;

public partial class GameCamera : Camera2D
{

	private readonly StringName ACTION_PAN_LEFT = "pan_left";
	private readonly StringName ACTION_PAN_RIGHT = "pan_right";
	private readonly StringName ACTION_PAN_UP = "pan_up";
	private readonly StringName ACTION_PAN_DOWN = "pan_down";

	private readonly float NOISE_SAMPLE_GROWTH = .1f;
	private readonly float MAX_CAMERA_OFFSET = 12f;
	private readonly float NOISE_FREQUANCY_MULTIPLIER = 100f;
	private readonly float SHAKE_DECAY = 2f;

	private readonly float PAN_SPEED = 550;

	private Vector2 noiseSample;
	private float currentShakePercentage = 0;

	private static GameCamera Instance;

	[Export] private FastNoiseLite shakeNoise;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{
		var movementVector = Input.GetVector(
			ACTION_PAN_LEFT, ACTION_PAN_RIGHT, ACTION_PAN_UP, ACTION_PAN_DOWN
		);
		GlobalPosition += movementVector * PAN_SPEED * (float)delta;

		var viewportRect = GetViewportRect();
		var halfWidth = viewportRect.Size.X / 2;
		var halfHeight = viewportRect.Size.Y / 2;
		float minX = LimitLeft + halfWidth;
		float maxX = LimitRight - halfWidth;

		float minY = LimitTop + halfHeight;
		float maxY = LimitBottom - halfHeight;
		float xClamped = (minX > maxX) ? (LimitLeft + LimitRight) / 2f : Mathf.Clamp(GlobalPosition.X, minX, maxX);
		float yClamped = (minY > maxY) ? (LimitTop + LimitBottom) / 2f : Mathf.Clamp(GlobalPosition.Y, minY, maxY);
		GlobalPosition = new Vector2(xClamped, yClamped);

		ApplyCameraShake(delta);
	}

	public static void Shake()
	{
		Instance.currentShakePercentage = 1;
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

	private void ApplyCameraShake(double delta)
	{
		if (currentShakePercentage == 0) return;

		noiseSample.X += NOISE_SAMPLE_GROWTH * NOISE_FREQUANCY_MULTIPLIER * (float)delta;
		noiseSample.Y += NOISE_SAMPLE_GROWTH * NOISE_FREQUANCY_MULTIPLIER * (float)delta;

		currentShakePercentage = Mathf.Clamp(currentShakePercentage - (SHAKE_DECAY * (float)delta), 0, 1);

		var xSample = shakeNoise.GetNoise2D(noiseSample.X, 0);
		var ySample = shakeNoise.GetNoise2D(0, noiseSample.Y);

		Offset = new Vector2(xSample, ySample) * MAX_CAMERA_OFFSET * currentShakePercentage;
	}
}
