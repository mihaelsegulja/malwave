using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public PackedScene BulletScene;
	[Export] public int Health = 3;
	
	private Sprite2D _aimArrow;
	private AnimatedSprite2D _anim;
	private bool _isHurt = false;
	
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

		if (_isHurt)
		{
			MoveAndSlide(); // still allow movement
			return;
		}
		if (direction != Vector2.Zero)
		{
			direction = direction.Normalized();
			Velocity = direction * Speed;
			_anim.Play("default");
		}
		else
		{
			Velocity = Vector2.Zero;
			_anim.Play("idle");
		}

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
		GD.Print("player damaged");
		Health--;

		_isHurt = true;
		_anim.Play("damage");

		GetTree().CreateTimer(0.3).Timeout += () =>
		{
			_isHurt = false;
			_anim.Play("default");
		};

		if (Health <= 0)
			Die();
	}

	public void Die()
	{
		GD.Print("Player died!");
		//EmitSignal("player_died");
		QueueFree();
	}
}
