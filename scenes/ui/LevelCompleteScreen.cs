using Game.AutoLoad;
using Godot;

namespace Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
  private Button nextLevelButton;

  public override void _Ready()
  {
    nextLevelButton = GetNode<Button>("%NextLevelButton");

    AudioHelpers.RegisterButtons([nextLevelButton]);
    AudioHelpers.PlayVictory();

    nextLevelButton.Pressed += OnNextLevelButtonPressed;
  }

  private void OnNextLevelButtonPressed()
  {
    LevelManager.Instance.ChangeToNextLevel();
  }
}
