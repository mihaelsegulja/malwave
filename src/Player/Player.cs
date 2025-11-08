using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;

	public override void _Process(double delta)
	{
		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("ui_right")) direction.X += 1;
		if (Input.IsActionPressed("ui_left")) direction.X -= 1;
		if (Input.IsActionPressed("ui_down")) direction.Y += 1;
		if (Input.IsActionPressed("ui_up")) direction.Y -= 1;
		GD.Print(direction);

		Position += direction * Speed * (float)delta;
		
		MoveAndSlide();
	}
}
