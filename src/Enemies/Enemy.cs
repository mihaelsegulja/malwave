using Godot;

public partial class Enemy : CharacterBody2D
{
  [Export] public float Speed = 100f;
  [Export] public float AttackCooldown = 1f;
  [Export] public int MaxHealth = 3;
  [Export] public PackedScene HealthBarScene;
  [Export] public PackedScene[] DropTable;
  [Export] public float DropChance = 0.2f;
  [Signal] public delegate void DiedEventHandler(Enemy e);
  public bool CountsForWave = true;

  protected Node2D Player;
  protected AnimatedSprite2D Anim;
  protected bool CanAttack = true;
  protected int Health;
  private HealthBar _healthBar;
  private static CircleShape2D _separationShape = new CircleShape2D { Radius = 16f };
  private PhysicsDirectSpaceState2D _space;

  public override void _Ready()
  {
	_space = GetWorld2D().DirectSpaceState;
	Player = GetTree().Root.FindChild("Player", true, false) as Node2D;
	Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	HealthBarScene ??= GD.Load<PackedScene>("res://src/UI/HealthBar/HealthBar.tscn");
	_healthBar = (HealthBar)HealthBarScene.Instantiate();
	AddChild(_healthBar);
	_healthBar.Position = new Vector2(-15, 20);
	Health = MaxHealth;
	_healthBar.SetMaxHealth(MaxHealth);
	_healthBar.SetHealth(Health);

	Anim.Play("default");
	Scale = new Vector2(1.25f, 1.25f);
  }

  public override void _PhysicsProcess(double delta)
  {
	if (!IsInstanceValid(Player))
	  return;

	ChasePlayer();
	HandleCollisions();
  }

  protected virtual void ChasePlayer()
  {
	Vector2 direction = (Player.GlobalPosition - GlobalPosition).Normalized();
	direction += SeparationVector() * 0.8f;
	direction = direction.Normalized();

	if (direction.X < 0)
	  Anim.FlipH = true;
	else if (direction.X > 0)
	  Anim.FlipH = false;

	Velocity = direction * Speed;
	MoveAndSlide();
  }

  private void HandleCollisions()
  {
	for (int i = 0; i < GetSlideCollisionCount(); i++)
	{
	  var collision = GetSlideCollision(i);
	  var collider = collision.GetCollider();

	  if (collider is Player player)
		TryDamage(player);
	}
  }

  protected virtual void TryDamage(Player player)
  {
	if (!CanAttack) return;

	player.TakeDamage();
	CanAttack = false;

	var t = GetTree().CreateTimer(AttackCooldown);
	t.Timeout += () => CanAttack = true;
  }

  public virtual void TakeDamage()
  {
	Health--;
	_healthBar.SetHealth(Health);

	Anim.Play("damage");

	if (Health <= 0)
	{
	  Die();
	  return;
	}

	GetTree().CreateTimer(0.2).Timeout += () => Anim.Play("default");
  }

  protected virtual void Die()
  {
	if (DropTable != null && DropTable.Length > 0 && GD.Randf() < DropChance)
	{
	  var scene = DropTable[GD.Randi() % DropTable.Length].Instantiate<Node2D>();
	  scene.Position = Position;
	  GetTree().CurrentScene.CallDeferred("add_child", scene);
	}

	EmitSignal(SignalName.Died, this);

	CallDeferred("queue_free");
  }

  private Vector2 SeparationVector()
  {
	var space = _space;

	PhysicsShapeQueryParameters2D p = new()
	{
	  Transform = new Transform2D(0, GlobalPosition),
	  CollisionMask = 1 << 3
	};

	p.SetShape(_separationShape);

	var hits = space.IntersectShape(p);

	Vector2 repulsion = Vector2.Zero;

	foreach (var hit in hits)
	{
	  var colliderVariant = hit["collider"];
	  Node colliderNode = colliderVariant.As<Node>();

	  if (colliderNode is Enemy other && other != this)
	  {
		repulsion += (GlobalPosition - other.GlobalPosition).Normalized();
	  }
	}

	return repulsion;
  }
}
