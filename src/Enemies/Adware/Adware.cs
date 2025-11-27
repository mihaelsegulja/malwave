using Godot;

public partial class Adware : Enemy
{
  protected override void ChasePlayer()
  {
    Velocity = Vector2.Zero;
  }
}
