using Godot;

public partial class ShieldPowerUp : PowerUp
{
  [Export] public float Duration = 2.0f;

  public override void Apply(Player player)
  {
	player.ActivateShield(Duration);
  }
}
