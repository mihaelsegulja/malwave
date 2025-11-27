using Godot;

public abstract partial class PowerUp : Area2D
{
  public abstract void Apply(Player player);

  public override void _Ready()
  {
    BodyEntered += body =>
    {
      if (body is Player p)
      {
        Apply(p);
        p.ReportPowerUpCollection();
        QueueFree();
      }
    };
  }
}
