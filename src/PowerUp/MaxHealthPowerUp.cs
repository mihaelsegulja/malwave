using Godot;
using System;

public partial class MaxHealthPowerUp : PowerUp
{
	[Export] public int Increase = 10;

	public override void Apply(Player player)
	{
		player.UpdateHealth(Increase, Increase);
	}
}
