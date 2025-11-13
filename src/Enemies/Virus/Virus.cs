using Godot;
using System;

public partial class Virus : Area2D
{
	[Export] public float Speed = 100f;
	[Export] public float AttackCooldown = 1f;
	
	private Node2D _player;
	private AnimatedSprite2D _anim;
	private bool _canAttack = true;

	public override void _Ready()
	{
		_player = GetTree().CurrentScene.GetNode<Node2D>("Player");
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_anim.Play("default");
		
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
	}

	public override void _PhysicsProcess(double delta)
	{
		// Skip if player no longer exists or was freed
		if (_player == null || !_player.IsInsideTree()) return;

		Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * Speed * (float)delta;
	}

	public void TakeDamage()
	{
		_anim.Play("damage");
		GetTree().CreateTimer(0.2).Timeout += () => _anim.Play("default");
	}
	
	private void OnBodyEntered(Node body)
	{
		if (!_canAttack) return;

		if (body is Player player)
		{
			_canAttack = false;
			player.TakeDamage();

			// reset attack cooldown
			var timer = GetTree().CreateTimer(AttackCooldown);
			timer.Timeout += () => _canAttack = true;
		}
	}
}
