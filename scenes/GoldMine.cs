using Godot;

namespace Game;


public partial class GoldMine : Node2D
{

  #region Resources
  [Export]
  private Texture2D activeTexture;
  #endregion

  #region Node references
  private Sprite2D sprite;
  #endregion

  public override void _Ready()
  {
    sprite = GetNode<Sprite2D>("Sprite2D");
  }

  public void SetActive()
  {
    sprite.Texture = activeTexture;
  }
}
