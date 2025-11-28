using Godot;

public partial class HealthBar : Control
{
  [Export] public int MaxHealth = 1;
  [Export] public int CurrentHealth = 1;

  private ColorRect _fill;
  private ColorRect _background;

  public override void _Ready()
  {
	_background = GetNode<ColorRect>("Background");
	_fill = GetNode<ColorRect>("Fill");

	MaxHealth = Mathf.Max(MaxHealth, 1);
	CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

	UpdateGraphics();
  }

  public void SetHealth(int value)
  {
	CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
	UpdateGraphics();
  }

  public void SetMaxHealth(int value, bool adjustCurrent = false)
  {
	MaxHealth = Mathf.Max(1, value);

	if (adjustCurrent)
	{
	  CurrentHealth = MaxHealth;
	}
	else
	{
	  CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
	}

	UpdateGraphics();
  }

  private void UpdateGraphics()
  {
	if (_background == null || _fill == null) return;

	float percent = (float)CurrentHealth / MaxHealth;
	_fill.Size = new Vector2(_background.Size.X * percent, _background.Size.Y);
	_fill.Position = Vector2.Zero;
  }
}
