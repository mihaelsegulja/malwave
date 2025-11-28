using Godot;
using System;

public partial class WaveManager : Node
{
  [Export] public PackedScene VirusScene;
  [Export] public PackedScene TrojanScene;
  [Export] public PackedScene AdwareScene;
  [Export] public Node2D[] SpawnPoints;
  [Export] public PackedScene WaveUIScene;

  private WaveUI _waveUI;
  private int _currentWave = 0;
  private int _aliveEnemies = 0;
  private int _totalEnemiesKilled = 0;
  private int _totalPowerUpsCollected = 0;
  private Player _player;
  private bool _nextWaveScheduled = false;
  private ArenaManager _arena;

  public override void _Ready()
  {
	_player = GetTree().CurrentScene.GetNode<Player>("Player");
	_arena = GetTree().CurrentScene.GetNode<ArenaManager>("Arena");

	if (WaveUIScene != null)
	{
	  _waveUI = WaveUIScene.Instantiate<WaveUI>();
	  GetTree().Root.AddChild(_waveUI);
	}

	ChildEnteredTree += OnChildAdded;

	StartNextWave();
  }

  private void OnChildAdded(Node n)
  {
	if (n is Enemy e)
	{
	  e.Died -= OnEnemyDied;
	  e.Died += OnEnemyDied;

	  if (e.CountsForWave)
	  {
		_aliveEnemies++;
		GD.Print("Registered enemy. Alive now: ", _aliveEnemies);
	  }
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
	_nextWaveScheduled = false;

	if (_player != null)
	{
	  _player.FireRate = Mathf.Max(0.05f, _player.FireRate * 0.965f);
	  GD.Print("New fire rate:", _player.FireRate);
	}

	WaveInfo info = GenerateWave(_currentWave);

	_aliveEnemies = 0;
	
	var tileMap = _arena.GetNode<TileMapLayer>("TileMapLayer");
	var usedRect = tileMap.GetUsedRect();
	Vector2I startTile = usedRect.Position;
	Vector2I direction = Vector2I.Down;
	_arena.StartWaveSweep(startTile, direction, speed: 15f);

	_waveUI?.ShowWave(_currentWave + 1);

	SpawnEnemies(info);

	_currentWave++;
	GD.Print("Wave: " + _currentWave);
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

	  enemy.Position = GetValidSpawnPosition();

	  CallDeferred("add_child", enemy);
	}
  }

  private void OnEnemyDied(Enemy e)
  {
	if (e is Trojan)
	{
	  SpawnAdditionalViruses(e.GlobalPosition);
	}

	if (!e.CountsForWave) return;

	_aliveEnemies--;
	_totalEnemiesKilled++;
	GD.Print("Alive now: ", _aliveEnemies);

	if (_aliveEnemies <= 0 && !_nextWaveScheduled)
	{
	  _nextWaveScheduled = true;
	  GD.Print("Wave " + _currentWave + " completed");
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

  private void SpawnAdditionalViruses(Vector2 origin)
  {
	int count = GD.RandRange(1, 2);

	for (int i = 0; i < count; i++)
	{
	  var virus = (Enemy)VirusScene.Instantiate();
	  virus.Position = origin + new Vector2(GD.Randf() * 16 - 8, GD.Randf() * 16 - 8);

	  virus.CountsForWave = true;

	  CallDeferred("add_child", virus);
	}
  }

  private Vector2 GetValidSpawnPosition()
  {
	for (int i = 0; i < 10; i++)
	{
	  var sp = SpawnPoints[GD.Randi() % SpawnPoints.Length];
	  var offset = new Vector2(
	  GD.Randf() * 48f - 24f,
	  GD.Randf() * 48f - 24f
	  );

	  Vector2 pos = sp.Position + offset;

	  PhysicsPointQueryParameters2D query = new()
	  {
		Position = pos,
		CollideWithAreas = false,
		CollideWithBodies = true
	  };

	  var hits = GetTree().Root.GetWorld2D().DirectSpaceState.IntersectPoint(query);

	  if (hits.Count == 0)
		return pos;
	}

	return SpawnPoints[0].Position;
  }

  public int GetTotalEnemiesKilled()
  {
	return _totalEnemiesKilled;
  }

  public int GetTotalPowerUpsCollected()
  {
	return _totalPowerUpsCollected;
  }

  public int GetCurrentWaveIndex()
  {
	return _currentWave;
  }

  public void CollectPowerUp()
  {
	_totalPowerUpsCollected++;
  }
}
