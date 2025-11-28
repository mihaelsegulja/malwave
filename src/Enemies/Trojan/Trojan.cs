using Godot;

public partial class Trojan : Enemy
{
  [Export] public PackedScene VirusScene;

  public override void _Ready()
  {
	MaxHealth = 5;
	base._Ready();
  }
}
