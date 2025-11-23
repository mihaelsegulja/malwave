using Godot;
using System;

public partial class HealthBar : Control
{
	[Export] public int MaxHealth = 1;
	[Export] public int CurrentHealth = 1;

	private ColorRect _fill;
	private ColorRect _background;
	private float _fullWidth;

	public override void _Ready()
	{
		_background = GetNode<ColorRect>("Background");
		_fill = GetNode<ColorRect>("Fill");

		_fullWidth = _background.Size.X;

		_fill.Visible = true;
		_background.Visible = true;

		UpdateGraphics();
	}

	public void SetHealth(int value)
	{
		CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
		UpdateGraphics();
	}

	public void SetMaxHealth(int value)
	{
		MaxHealth = value;
		if (CurrentHealth == 0)
		{
			CurrentHealth = MaxHealth;
		}
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
		UpdateGraphics();
	}

	private void UpdateGraphics()
	{
		if (MaxHealth <= 0) return;

		float percent = (float)CurrentHealth / MaxHealth;

		// Set width proportional to health
		_fill.Size = new Vector2(_fullWidth * percent, _background.Size.Y);
		_fill.Position = new Vector2(0, 0);
	}
}
