using Godot;
using System;

public partial class Bullet : Area2D
{
	[Export] public float Speed = 500;
	public Vector2 Direction = Vector2.Up;

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * (float)delta * Speed;
	}

	private void _on_body_entered(Node2D body)
	{
		QueueFree();
	}
	
	public override void _Ready()
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");

		// assuming all frames are 32x32
		int index = GD.RandRange(0, 3);
		sprite.RegionEnabled = true;
		sprite.RegionRect = new Rect2(index * 32, 0, 32, 32);
	}

}
