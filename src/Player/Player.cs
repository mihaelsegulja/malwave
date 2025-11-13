using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public PackedScene BulletScene;
	[Export] public int Health = 3;
	
	private Sprite2D _aimArrow;
	private AnimatedSprite2D _anim;
	
	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_aimArrow = GetNode<Sprite2D>("AimArrow");
	}

	public override void _Process(double delta)
	{
		HandleMovement(delta);
		HandleAiming();
		HandleShooting();
	}

	private void HandleMovement(double delta)
	{
		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("ui_right")) direction.X += 1;
		if (Input.IsActionPressed("ui_left")) direction.X -= 1;
		if (Input.IsActionPressed("ui_down")) direction.Y += 1;
		if (Input.IsActionPressed("ui_up")) direction.Y -= 1;
		
		if (direction.X < 0)
			_anim.FlipH = true;
		else if (direction.X > 0)
			_anim.FlipH = false;
		
		if (direction != Vector2.Zero)
		{
			direction = direction.Normalized();
			Position += direction * Speed * (float)delta;
			_anim.Play("default");
		}
		else
		{
			_anim.Play("idle");
		}
		
		Velocity = direction.Normalized() * Speed;
		MoveAndSlide();
	}

	private void HandleAiming()
	{
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 dir = (mousePos - GlobalPosition).Normalized();

		// rotate the arrow to face the cursor
		_aimArrow.Rotation = dir.Angle() + Mathf.Pi / 2;

		// keep arrow circling the player
		float distance = 30f; // how far from player center
		_aimArrow.Position = dir * distance;
	}

	private void HandleShooting()
	{
		if (Input.IsActionJustPressed("shoot"))
			Shoot();
	}

	private void Shoot()
	{
		var bullet = (Bullet)BulletScene.Instantiate();
		bullet.Position = GlobalPosition;

		Vector2 mousePos = GetGlobalMousePosition();
		bullet.Direction = (mousePos - GlobalPosition).Normalized();
		bullet.Rotation = bullet.Direction.Angle();

		GetTree().CurrentScene.AddChild(bullet);
	}
	
	public void TakeDamage()
	{
		Health--;
		_anim.Play("damage");

		GetTree().CreateTimer(0.2).Timeout += () => _anim.Play("default");

		if (Health <= 0)
			Die();
	}

	private void Die()
	{
		GD.Print("Player died!");
		SetProcess(false);
		SetPhysicsProcess(false);
		//QueueFree();
	}
}
