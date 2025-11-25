using Godot;
using System;

public partial class ShieldPowerUp : PowerUp
{
	[Export] public float Duration = 3.0f;

	public override void Apply(Player player)
	{
		player.ActivateShield(Duration);
	}
}
