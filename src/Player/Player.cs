using Godot;

public partial class Player : CharacterBody2D
{
  [Export] public float Speed = 200f;
  [Export] public int MaxHealth = 5;
  [Export] public float FireRate = 0.2f;
  [Export] public float AimDeadzone = 0.35f;
  [Export] public float AimDistance = 30f;
  [Export] public PackedScene BulletScene;
  [Export] public PackedScene HealthBarScene;
  [Export] public PackedScene PauseMenuScene;
  [Export] public PackedScene GameOverUIScene;

  public bool ShieldActive = false;
  public int Health;

  private Sprite2D _aimArrow;
  private AnimatedSprite2D _anim;
  private bool _isHurt = false;
  private float _shootCooldown = 0f;
  private HealthBar _healthBar;
  private float shieldTimer = 0.0f;
  private Polygon2D _shieldVisual;
  private CanvasLayer _activePauseMenu;
  private AudioStreamPlayer _bulletSFX;
  private AudioStreamPlayer _powerUpSFX;

  public override void _Ready()
  {
	ProcessMode = Node.ProcessModeEnum.Pausable;
	PauseMenuScene = GD.Load<PackedScene>("res://src/UI/MainMenu/MainMenu.tscn");
	_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	_aimArrow = GetNode<Sprite2D>("AimArrow");
	HealthBarScene ??= GD.Load<PackedScene>("res://src/UI/HealthBar/HealthBar.tscn");
	GameOverUIScene = GD.Load<PackedScene>("res://src/UI/GameOverUI/GameOverUI.tscn");
	_healthBar = (HealthBar)HealthBarScene.Instantiate();
	AddChild(_healthBar);
	_healthBar.Position = new Vector2(-15, 20);
	Health = MaxHealth;
	_healthBar.SetMaxHealth(MaxHealth);
	_healthBar.SetHealth(Health);
	_shieldVisual = GetNode<Polygon2D>("Shield");
	_shieldVisual.Visible = false;
	_bulletSFX = GetNodeOrNull<AudioStreamPlayer>("BulletSFX");
	_powerUpSFX = GetNodeOrNull<AudioStreamPlayer>("PowerUpSFX");

	Scale = new Vector2(1.25f, 1.25f);
  }

  public override void _PhysicsProcess(double delta)
  {
	HandleMovement((float)delta);
	if (_shootCooldown > 0f) _shootCooldown -= (float)delta;
  }

  public override void _Process(double delta)
  {
	HandleAiming();
	HandleShooting((float)delta);

	if (ShieldActive)
	{
	  shieldTimer -= (float)delta;
	  if (shieldTimer <= 0)
	  {
		ShieldActive = false;
		_shieldVisual.Visible = false;
	  }
	}
  }

  private void HandleMovement(float delta)
  {
	Vector2 direction = Vector2.Zero;

	if (Input.IsActionPressed("ui_right")) direction.X += 1;
	if (Input.IsActionPressed("ui_left")) direction.X -= 1;
	if (Input.IsActionPressed("ui_down")) direction.Y += 1;
	if (Input.IsActionPressed("ui_up")) direction.Y -= 1;

	if (direction.X < 0) _anim.FlipH = true;
	else if (direction.X > 0) _anim.FlipH = false;

	if (_isHurt)
	{
	  Velocity = direction.Normalized() * Speed;
	  MoveAndSlide();
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
	float ax = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
	float ay = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
	Vector2 gamepadAim = new Vector2(ax, ay);

	Vector2 aimDir;

	if (gamepadAim.Length() >= AimDeadzone)
	{
	  aimDir = gamepadAim.Normalized();
	}
	else
	{
	  Vector2 mousePos = GetGlobalMousePosition();
	  aimDir = (mousePos - GlobalPosition).Normalized();
	}

	_aimArrow.Rotation = aimDir.Angle() + Mathf.Pi / 2;
	_aimArrow.Position = aimDir * AimDistance;
  }

  private void HandleShooting(float delta)
  {
	bool firePressed = Input.IsActionPressed("shoot");

	float ax = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
	float ay = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
	Vector2 gamepadAim = new Vector2(ax, ay);
	bool gpAiming = gamepadAim.Length() >= AimDeadzone;

	if (firePressed || gpAiming)
	{
	  TryShoot(gamepadAim);
	}
  }

  private void TryShoot(Vector2 gamepadAim)
  {
	if (_shootCooldown > 0f) return;

	Vector2 aimDir;
	if (gamepadAim.Length() >= AimDeadzone)
	  aimDir = gamepadAim.Normalized();
	else
	  aimDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();

	Shoot(aimDir);
	_shootCooldown = FireRate;
  }

  private void Shoot(Vector2 direction)
  {
	if (BulletScene == null)
	{
	  GD.PrintErr("BulletScene not set on Player");
	  return;
	}
	
	_bulletSFX?.Play();

	var bullet = (Bullet)BulletScene.Instantiate();
	bullet.Position = GlobalPosition;
	bullet.Direction = direction;
	bullet.Rotation = direction.Angle();

	GetTree().CurrentScene.AddChild(bullet);
  }

  public void TakeDamage()
  {
	if (ShieldActive) return;

	GD.Print("player damaged");
	Health--;
	_healthBar.SetHealth(Health);

	_isHurt = true;
	_anim.Play("damage");

	GetTree().CreateTimer(0.3).Timeout += () =>
	{
	  _isHurt = false;
	  _anim.Play("default");
	};

	if (Health <= 0) Die();
  }

  public void Die()
  {
	if (GetTree().CurrentScene.GetNodeOrNull<WaveManager>("WaveManager") is WaveManager waveManager)
	{
	  if (GameOverUIScene != null)
	  {
		var gameOverUI = GameOverUIScene.Instantiate<GameOverUI>();
		GetTree().Root.AddChild(gameOverUI);

		gameOverUI.ShowGameOver(
		  waveReached: waveManager.GetCurrentWaveIndex(),
		  enemiesKilled: waveManager.GetTotalEnemiesKilled(),
		  powerUpsCollected: waveManager.GetTotalPowerUpsCollected()
		);
	  }
	}

	GD.Print("Player died!");
	QueueFree();
  }

  public void ActivateShield(float duration)
  {
	_shieldVisual.Visible = true;
	ShieldActive = true;
	shieldTimer = duration;
  }

  public void UpdateHealth(int amount, int? newMax = null)
  {
	if (newMax.HasValue)
	{
	  MaxHealth += newMax.Value;
	  Health = Mathf.Min(Health, MaxHealth);
	  _healthBar.SetMaxHealth(MaxHealth, adjustCurrent: false);
	}

	Health = Mathf.Clamp(Health + amount, 0, MaxHealth);
	_healthBar.SetHealth(Health);

	GD.Print("Health: " + Health);
	GD.Print("MaxHealth: " + MaxHealth);
  }

  public override void _Input(InputEvent @event)
  {
	if (@event.IsActionPressed("ui_cancel"))
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
	if (GetTree().CurrentScene.GetNodeOrNull<WaveManager>("WaveManager") is WaveManager waveManager)
	{
	  waveManager.CollectPowerUp();
	}
  }
}
