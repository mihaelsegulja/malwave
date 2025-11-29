using Godot;

public partial class Bullet : Area2D
{
    [Export] public float Speed { get; set; } = 500f;
    [Export] public Vector2 Direction { get; set; } = Vector2.Up;

    private bool _hasHit;

    public override void _Ready()
    {
        SetupRandomSprite();

        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hasHit)
            return;

        Position += Direction * (float)delta * Speed;
    }

    private void SetupRandomSprite()
    {
        var sprite = GetNode<Sprite2D>("Sprite2D");

        sprite.RegionEnabled = true;
        uint index = GD.Randi() % 4;
        sprite.RegionRect = new Rect2(index * 32, 0, 32, 32);
    }

    private void OnBodyEntered(Node body)
    {
        if (_hasHit)
            return;

        if (body is Enemy enemy)
        {
            enemy.TakeDamage();
            _hasHit = true;
            QueueFree();
        }
    }
}
