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
	
	public override void _Ready()
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		Connect("body_entered", new Callable(this, nameof(_on_body_entered)));
		
		int index = GD.RandRange(0, 3);
		sprite.RegionEnabled = true;
		sprite.RegionRect = new Rect2(index * 32, 0, 32, 32);
	}
	
	private void _on_body_entered(Node body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDamage();
			QueueFree();
		}
	}

}
