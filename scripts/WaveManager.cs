using Godot;
using System;
using System.Collections.Generic;

public partial class WaveManager : Node
{
	[Export] public PackedScene VirusScene { get; set; }
	[Export] public PackedScene TrojanScene { get; set; }
	[Export] public PackedScene AdwareScene { get; set; }

	[Export] public Node2D[] SpawnPoints { get; set; }

	private int _currentWave = 0;
	private int _aliveEnemies = 0;
	private Player _player;

	public override void _Ready()
	{
		_player = GetTree().CurrentScene.GetNode<Player>("Player");
		ChildEnteredTree += OnChildAdded;
		StartNextWave();
	}
	
	private void OnChildAdded(Node n)
	{
		if (n is Enemy e)
		{
			e.Died += OnEnemyDied;
			_aliveEnemies++;
		}
	}

	private struct WaveInfo
	{
		public int Virus;
		public int Trojan;
		public int Adware;

		public WaveInfo(int virus, int trojan, int adware)
		{
			Virus = virus;
			Trojan = trojan;
			Adware = adware;
		}

		public int Total => Virus + Trojan + Adware;
	}
	
	private void StartNextWave()
	{
		if (_player != null)
		{
			_player.FireRate = Mathf.Max(0.05f, _player.FireRate * 0.92f);
			GD.Print("New fire rate:", _player.FireRate);
		}

		WaveInfo info = GenerateWave(_currentWave);
		_aliveEnemies = 0;
		SpawnEnemies(info);
		
		_currentWave++;
	}

	private void SpawnEnemies(WaveInfo info)
	{
		Spawn(VirusScene, info.Virus);
		Spawn(TrojanScene, info.Trojan);
		Spawn(AdwareScene, info.Adware);
	}

	private void Spawn(PackedScene scene, int count)
	{
		for (int i = 0; i < count; i++)
		{
			var enemy = scene.Instantiate<Node2D>();

			var sp = SpawnPoints[GD.Randi() % SpawnPoints.Length];
			enemy.Position = sp.Position;

			AddChild(enemy);
		}
	}
	
	private void OnEnemyDied()
	{
		_aliveEnemies--;
		if (_aliveEnemies <= 0)
		{
			GetTree().CreateTimer(1.5).Timeout += StartNextWave;
		}
	}
	
	private WaveInfo GenerateWave(int waveIndex)
	{
		float difficulty = 1.0f + waveIndex * 0.25f;

		int baseVirus = 2 + (int)(difficulty * 2);
		int baseTrojan = (waveIndex >= 2) ? (int)(difficulty * 0.7f) : 0;
		int baseAdware = (waveIndex >= 4) ? (int)(difficulty * 0.4f) : 0;

		int virus = GD.RandRange(baseVirus - 1, baseVirus + 2);
		int trojan = GD.RandRange(baseTrojan - 1, baseTrojan + 1);
		int adware = GD.RandRange(baseAdware - 1, baseAdware + 1);

		virus = Math.Max(virus, 0);
		trojan = Math.Max(trojan, 0);
		adware = Math.Max(adware, 0);

		return new WaveInfo(virus, trojan, adware);
	}
}
