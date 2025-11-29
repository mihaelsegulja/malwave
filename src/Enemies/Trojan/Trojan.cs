using Godot;

public partial class Trojan : Enemy
{
    [Export] public PackedScene VirusScene { get; set; }

    public override void _Ready()
    {
        MaxHealth = 5;
        base._Ready();
    }
}
