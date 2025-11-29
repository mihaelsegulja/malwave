using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 100f;
    [Export] public float AttackCooldown { get; set; } = 1f;
    [Export] public int MaxHealth { get; set; } = 3;

    [Export] public PackedScene HealthBarScene { get; set; }
    [Export] public PackedScene[] DropTable { get; set; } = System.Array.Empty<PackedScene>();
    [Export] public float DropChance { get; set; } = 0.2f;

    [Signal] public delegate void DiedEventHandler(Enemy enemy);

    public bool CountsForWave { get; set; } = true;

    protected Node2D Player;
    protected AnimatedSprite2D Anim;

    protected bool CanAttack = true;
    protected int Health;

    private HealthBar _healthBar;

    // reused static shape avoids allocations
    private static readonly CircleShape2D SeparationShape = new() { Radius = 16f };

    private PhysicsDirectSpaceState2D _space;

    public override void _Ready()
    {
        _space = GetWorld2D().DirectSpaceState;

        Player = GetTree().Root.FindChild("Player", recursive: true, owned: false) as Node2D;
        Anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        HealthBarScene ??= GD.Load<PackedScene>("res://src/UI/HealthBar/HealthBar.tscn");

        _healthBar = HealthBarScene.Instantiate<HealthBar>();
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

        Anim.FlipH = direction.X < 0;

        Velocity = direction * Speed;
        MoveAndSlide();
    }

    private void HandleCollisions()
    {
        int count = GetSlideCollisionCount();
        for (int i = 0; i < count; i++)
        {
            var collision = GetSlideCollision(i);
            if (collision.GetCollider() is Player player)
            {
                TryDamage(player);
            }
        }
    }

    protected virtual void TryDamage(Player player)
    {
        if (!CanAttack)
            return;

        player.TakeDamage();
        CanAttack = false;

        var timer = GetTree().CreateTimer(AttackCooldown);
        timer.Timeout += () => CanAttack = true;
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
        if (DropTable.Length > 0 && GD.Randf() < DropChance)
        {
            var drop = DropTable[GD.Randi() % DropTable.Length].Instantiate<Node2D>();
            drop.Position = Position;
            GetTree().CurrentScene.CallDeferred(MethodName.AddChild, drop);
        }

        EmitSignal(SignalName.Died, this);

        CallDeferred(MethodName.QueueFree);
    }

    private Vector2 SeparationVector()
    {
        var query = new PhysicsShapeQueryParameters2D
        {
            Transform = new Transform2D(0, GlobalPosition),
            CollisionMask = 1 << 3
        };

        query.SetShape(SeparationShape);

        var hits = _space.IntersectShape(query);

        Vector2 repulsion = Vector2.Zero;

        foreach (var hit in hits)
        {
            if (hit["collider"].As<Node>() is Enemy other && other != this)
            {
                repulsion += (GlobalPosition - other.GlobalPosition).Normalized();
            }
        }

        return repulsion;
    }
}
