using Godot;
using System;

public partial class Adware : Enemy
{
	protected override void ChasePlayer()
	{
		// Do nothing = stand still
		Velocity = Vector2.Zero;
	}
}
