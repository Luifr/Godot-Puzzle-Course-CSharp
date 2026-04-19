using System.Collections.Generic;
using System.Linq;
using Game.UI;
using Godot;

namespace Game.Manager;

public partial class ResourceIndicatorManager : Node
{
	[Export] GridManager gridManager;
	[Export] PackedScene resourceIndicatorScene;

	private AudioStreamPlayer audioStreamPlayer;
	private HashSet<Vector2I> indicatedTiles = new();
	private Dictionary<Vector2I, ResourceIndicator> resourceIndicatorsHash = new();

	public override void _Ready()
	{
		audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

		gridManager.ResourceTilesUpdated += OnResourceTilesUpdate;
	}

	private void UpdateIndicators(IEnumerable<Vector2I> newIndicatedTiles, IEnumerable<Vector2I> toRemoveTiles)
	{
		foreach (var newTile in newIndicatedTiles)
		{
			var indicator = resourceIndicatorScene.Instantiate<ResourceIndicator>();
			AddChild(indicator);
			indicator.GlobalPosition = newTile * GridManager.TILE_SIZE;
			resourceIndicatorsHash[newTile] = indicator;
		}

		foreach (var tileToRemove in toRemoveTiles)
		{
			resourceIndicatorsHash.TryGetValue(tileToRemove, out var indicator);

			if (IsInstanceValid(indicator))
				indicator.Destroy();
			resourceIndicatorsHash.Remove(tileToRemove);
		}
	}

	private void OnResourceTilesUpdate(int _)
	{
		Callable.From(() =>
		{
			var currentResourceTiles = gridManager.GetCollectedResourceTiles();
			var newlyIndicatedTiles = currentResourceTiles.Except(indicatedTiles).ToList();
			var toRemoveTiles = indicatedTiles.Except(currentResourceTiles).ToList();
			indicatedTiles = currentResourceTiles;

			if (newlyIndicatedTiles.Count > 0) audioStreamPlayer.Play();

			UpdateIndicators(newlyIndicatedTiles, toRemoveTiles);
		}).CallDeferred();
	}
}
