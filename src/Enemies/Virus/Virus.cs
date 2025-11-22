using Godot;
using System;

public partial class Virus : Enemy
{
	public override void _Ready()
	{
		MaxHealth = 2;
		base._Ready();
	}
}
