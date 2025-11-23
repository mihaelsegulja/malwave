using Godot;
using System;

public partial class Trojan : Enemy
{
	[Export] public PackedScene VirusScene;

	public override void _Ready()
	{
		MaxHealth = 5;
		base._Ready();
	}

	protected override void Die()
	{
		CallDeferred(nameof(SpawnViruses));
		base.Die();
	}

	private void SpawnViruses()
	{
		int count = GD.RandRange(1, 2);  // spawns 1 or 2

		for (int i = 0; i < count; i++)
		{
			var virus = (Virus)VirusScene.Instantiate();
			virus.Position = GlobalPosition + new Vector2(GD.Randf() * 16 - 8, GD.Randf() * 16 - 8);
			GetParent().AddChild(virus);
		}
	}
}
