using Godot;

public partial class HealPowerUp : PowerUp
{
  [Export] public int Amount = 20;

  public override void Apply(Player player)
  {
    player.UpdateHealth(Amount);
  }
}
