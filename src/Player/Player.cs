using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; private set; } = 200f;
    [Export] public int MaxHealth { get; private set; } = 5;
    [Export] public float FireRate { get; private set; } = 0.2f;
    [Export] public float AimDeadzone { get; private set; } = 0.35f;
    [Export] public float AimDistance { get; private set; } = 30f;
    [Export] public PackedScene BulletScene { get; private set; }
    [Export] public PackedScene HealthBarScene { get; private set; }
    [Export] public PackedScene PauseMenuScene { get; private set; }
    [Export] public PackedScene GameOverUIScene { get; private set; }

    public int Health { get; private set; }
    public bool ShieldActive { get; private set; }

    private float _shootCooldown;
    private float _shieldTimer;
    private bool _isHurt;

    private AnimatedSprite2D _anim;
    private Sprite2D _aimArrow;
    private Polygon2D _shieldVisual;
    private HealthBar _healthBar;

    private AudioStreamPlayer _bulletSFX;
    private AudioStreamPlayer _powerUpSFX;

    private CanvasLayer _activePauseMenu;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Pausable;

        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _aimArrow = GetNode<Sprite2D>("AimArrow");
        _shieldVisual = GetNode<Polygon2D>("Shield");
        _shieldVisual.Visible = false;

        _bulletSFX = GetNodeOrNull<AudioStreamPlayer>("BulletSFX");
        _powerUpSFX = GetNodeOrNull<AudioStreamPlayer>("PowerUpSFX");

        SetupHealthBar();
        Health = MaxHealth;

        Scale = new Vector2(1.25f, 1.25f);
    }

    private void SetupHealthBar()
    {
        HealthBarScene ??= GD.Load<PackedScene>("res://src/UI/HealthBar/HealthBar.tscn");
        GameOverUIScene ??= GD.Load<PackedScene>("res://src/UI/GameOverUI/GameOverUI.tscn");
        PauseMenuScene ??= GD.Load<PackedScene>("res://src/UI/MainMenu/MainMenu.tscn");

        _healthBar = HealthBarScene.Instantiate<HealthBar>();
        AddChild(_healthBar);
        _healthBar.Position = new Vector2(-15, 20);

        _healthBar.SetMaxHealth(MaxHealth, adjustCurrent: true);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement((float)delta);
        if (_shootCooldown > 0f)
            _shootCooldown -= (float)delta;
    }

    public override void _Process(double delta)
    {
        HandleAiming();
        HandleShooting();

        if (ShieldActive)
        {
            _shieldTimer -= (float)delta;
            if (_shieldTimer <= 0f)
                DisableShield();
        }
    }

    private void HandleMovement(float delta)
    {
        Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        if (dir.X < 0) _anim.FlipH = true;
        else if (dir.X > 0) _anim.FlipH = false;

        if (_isHurt)
        {
            Velocity = dir * Speed;
            MoveAndSlide();
            return;
        }

        if (dir != Vector2.Zero)
        {
            Velocity = dir * Speed;
            _anim.Play("default");
        }
        else
        {
            Velocity = Vector2.Zero;
            _anim.Play("idle");
        }

        MoveAndSlide();
    }

    private Vector2 GetAimDirection()
    {
        Vector2 gp = new(
            Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left"),
            Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up")
        );

        if (gp.Length() >= AimDeadzone)
            return gp.Normalized();

        return (GetGlobalMousePosition() - GlobalPosition).Normalized();
    }

    private void HandleAiming()
    {
        Vector2 aim = GetAimDirection();
        _aimArrow.Rotation = aim.Angle() + Mathf.Pi / 2f;
        _aimArrow.Position = aim * AimDistance;
    }

    private void HandleShooting()
    {
        bool wantsToShoot = Input.IsActionPressed("shoot");

        if (!wantsToShoot && InputVectorAimBelowDeadzone()) 
            return;

        TryShoot();
    }

    private bool InputVectorAimBelowDeadzone()
    {
        float ax = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
        float ay = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
        return new Vector2(ax, ay).Length() < AimDeadzone;
    }

    private void TryShoot()
    {
        if (_shootCooldown > 0f) 
            return;

        Vector2 aim = GetAimDirection();
        Shoot(aim);

        _shootCooldown = FireRate;
    }

    private void Shoot(Vector2 direction)
    {
        if (BulletScene == null)
        {
            GD.PrintErr("BulletScene not assigned.");
            return;
        }

        _bulletSFX?.Play();

        var bullet = BulletScene.Instantiate<Bullet>();
        bullet.GlobalPosition = GlobalPosition;
        bullet.Direction = direction;
        bullet.Rotation = direction.Angle();

        GetTree().CurrentScene.AddChild(bullet);
    }

    public void TakeDamage()
    {
        if (ShieldActive)
            return;

        Health = Mathf.Max(Health - 1, 0);
        _healthBar.SetHealth(Health);

        _isHurt = true;
        _anim.Play("damage");

        GetTree().CreateTimer(0.3).Timeout += () =>
        {
            _isHurt = false;
            _anim.Play("default");
        };

        if (Health == 0)
            Die();
    }

    private void Die()
    {
        var waveManager = GetTree().CurrentScene.GetNodeOrNull<WaveManager>("WaveManager");
        if (waveManager != null && GameOverUIScene != null)
        {
            var ui = GameOverUIScene.Instantiate<GameOverUI>();
            GetTree().Root.AddChild(ui);

            ui.ShowGameOver(
                waveManager.GetCurrentWaveIndex(),
                waveManager.GetTotalEnemiesKilled(),
                waveManager.GetTotalPowerUpsCollected()
            );
        }

        QueueFree();
    }
    
    public void AdjustFireRate(float factor)
    {
        FireRate = Mathf.Max(0.05f, FireRate * factor);
    }

    public void ActivateShield(float duration)
    {
        ShieldActive = true;
        _shieldTimer = duration;
        _shieldVisual.Visible = true;
    }

    private void DisableShield()
    {
        ShieldActive = false;
        _shieldVisual.Visible = false;
    }

    public void UpdateHealth(int amount, int? increaseMax = null)
    {
        if (increaseMax.HasValue)
        {
            MaxHealth += increaseMax.Value;
            Health += increaseMax.Value;
            _healthBar.SetMaxHealth(MaxHealth, adjustCurrent: true);
        }

        Health = Mathf.Clamp(Health + amount, 0, MaxHealth);
        _healthBar.SetHealth(Health);
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
        {
            if (_activePauseMenu != null && IsInstanceValid(_activePauseMenu))
            {
                GetTree().Paused = false;
                _activePauseMenu.QueueFree();
                _activePauseMenu = null;
            }
            else
            {
                GetTree().Paused = true;
                _activePauseMenu = PauseMenuScene.Instantiate<CanvasLayer>();
                _activePauseMenu.ProcessMode = Node.ProcessModeEnum.Always;
                _activePauseMenu.Set("IsPauseMenu", true);
                GetTree().Root.AddChild(_activePauseMenu);
            }
        }
    }

    public void ReportPowerUpCollection()
    {
        _powerUpSFX?.Play();

        var wm = GetTree().CurrentScene.GetNodeOrNull<WaveManager>("WaveManager");
        wm?.CollectPowerUp();
    }
}
