using Game.Component;
using Godot;

namespace Game.Building;

public partial class GoblinCamp : Node2D
{
	[Export] BuildingComponent buildingComponent;
	[Export] private AnimatedSprite2D animatedSprite2D;
	[Export] private AnimatedSprite2D fireAnimatedSprite2D;


	public override void _Ready()
	{
		fireAnimatedSprite2D.Hide();

		buildingComponent.Enabled += OnEnabled;
		buildingComponent.Disabled += OnDisabled;
	}

	private void OnEnabled()
	{
		animatedSprite2D.Play("default");
		fireAnimatedSprite2D.Hide();
	}

	private void OnDisabled()
	{
		animatedSprite2D.Play("destroyed");
		fireAnimatedSprite2D.Show();
	}
}
