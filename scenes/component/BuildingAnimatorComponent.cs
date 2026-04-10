using System.Linq;
using Godot;

namespace Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
	[Signal] public delegate void DestroyAnimationFinishedEventHandler();


	[Export] private PackedScene impactParticleScene;
	[Export] private PackedScene destroyParticleScene;
	[Export] private Texture2D maskTexture;

	private Tween activeTween;
	private Node2D animationRootNode;
	private Sprite2D maskNode;

	public override void _Ready()
	{
		YSortEnabled = false;
		SetupNodes();
	}

	public bool IsPlayingAnimation()
	{
		return activeTween != null && activeTween.IsRunning();
	}

	public void PlayInAnimation()
	{
		if (animationRootNode == null) return;
		if (activeTween != null && activeTween.IsValid())
		{
			activeTween.Kill();
		}

		activeTween = CreateTween();
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.Position.ToString(), Vector2.Zero, 0.3f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In)
			.From(Vector2.Up * 128);

		activeTween.TweenCallback(Callable.From(() =>
		{
			var impactParticles = impactParticleScene.Instantiate<Node2D>();
			Owner.GetParent().AddChild(impactParticles);
			impactParticles.GlobalPosition = GlobalPosition;
		}));

		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.Position.ToString(), Vector2.Up * 16, 0.1f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.Position.ToString(), Vector2.Zero, 0.1f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
	}

	public void PlayDestroyAnimation()
	{
		if (animationRootNode == null) return;
		if (activeTween != null && activeTween.IsValid())
		{
			activeTween.Kill();
		}

		maskNode.Texture = maskTexture;
		maskNode.ClipChildren = ClipChildrenMode.Only;

		var destroyParticles = destroyParticleScene.Instantiate<Node2D>();
		Owner.GetParent().AddChild(destroyParticles);
		destroyParticles.GlobalPosition = GlobalPosition;

		activeTween = CreateTween();
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.RotationDegrees.ToString(), -5, 0.1f);
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.RotationDegrees.ToString(), 5, 0.1f);
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.RotationDegrees.ToString(), -2, 0.1f);
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.RotationDegrees.ToString(), 2, 0.1f);
		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.RotationDegrees.ToString(), 0, 0.1f);

		activeTween
			.TweenProperty(animationRootNode, Node2D.PropertyName.Position.ToString(), Vector2.Down * 300, 0.4f)
			.SetTrans(Tween.TransitionType.Quart)
			.SetEase(Tween.EaseType.In);

		activeTween.Finished += EmitSignalDestroyAnimationFinished;
	}

	private void SetupNodes()
	{
		var spriteNode = GetChildren().FirstOrDefault() as Node2D;
		if (spriteNode == null) return;

		RemoveChild(spriteNode);
		Position = new Vector2(spriteNode.Position.X, spriteNode.Position.Y);

		var maskTextureSize = maskTexture.GetSize();

		maskNode = new Sprite2D
		{
			Centered = false,
			Offset = new Vector2(-maskTextureSize.X / 2, -maskTextureSize.Y),
		};
		AddChild(maskNode);

		animationRootNode = new Node2D();
		maskNode.AddChild(animationRootNode);

		animationRootNode.AddChild(spriteNode);
		spriteNode.Position = new Vector2(0, 0);
	}
}
