using Godot;

public partial class HealPowerUp : PowerUp
{
  [Export] public int Amount = 2;

  public override void Apply(Player player)
  {
	player.UpdateHealth(Amount);
  }
}
