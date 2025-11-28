using Godot;

public partial class MaxHealthPowerUp : PowerUp
{
  [Export] public int Increase = 1;

  public override void Apply(Player player)
  {
	player.UpdateHealth(Increase, Increase);
  }
}
