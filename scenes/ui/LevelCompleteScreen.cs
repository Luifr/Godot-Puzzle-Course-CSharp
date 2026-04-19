using Game.AutoLoad;
using Godot;

namespace Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
  [Export(PropertyHint.File, "*.tscn")]
  private string mainMenuScenePath;

  private Button nextLevelButton;

  public override void _Ready()
  {
    nextLevelButton = GetNode<Button>("%NextLevelButton");

    AudioHelpers.RegisterButtons([nextLevelButton]);
    AudioHelpers.PlayVictory();

    if (LevelManager.IsLastLevel())
    {
      nextLevelButton.Text = "Return to Menu";
    }

    nextLevelButton.Pressed += OnNextLevelButtonPressed;
  }

  private void OnNextLevelButtonPressed()
  {
    if (LevelManager.IsLastLevel())
    {
      GetTree().ChangeSceneToFile(mainMenuScenePath);
    }
    else
    {
      LevelManager.ChangeToNextLevel();
    }
  }
}
