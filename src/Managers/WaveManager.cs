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
    private Player _player;
    private ArenaManager _arena;

    private int _currentWave = 0;
    private int _aliveEnemies = 0;
    private int _totalEnemiesKilled = 0;
    private int _totalPowerUpsCollected = 0;
    private bool _nextWaveScheduled = false;

    public override void _Ready()
    {
        _player = GetTree().CurrentScene.GetNode<Player>("Player");
        _arena = GetTree().CurrentScene.GetNode<ArenaManager>("Arena");

        SetupUI();
        ChildEnteredTree += OnChildAdded;

        StartNextWave();
    }

    private void SetupUI()
    {
        if (WaveUIScene == null) return;

        _waveUI = WaveUIScene.Instantiate<WaveUI>();
        GetTree().Root.AddChild(_waveUI);
    }

    private void StartNextWave()
    {
        _nextWaveScheduled = false;

        AdjustDifficulty();
        TriggerArenaSweep();
        ShowWaveUI();

        WaveInfo info = GenerateWave(_currentWave);
        _aliveEnemies = 0;

        SpawnWave(info);

        _currentWave++;
        GD.Print("Wave: ", _currentWave);
    }

    private void AdjustDifficulty()
    {
        if (_player == null) return;

        _player.AdjustFireRate(0.965f);
        GD.Print("New fire rate: ", _player.FireRate);
    }

    private void TriggerArenaSweep()
    {
        var tileMap = _arena.GetNode<TileMapLayer>("TileMapLayer");

        var usedRect = tileMap.GetUsedRect();
        Vector2I startTile = usedRect.Position;

        _arena.StartWaveSweep(startTile, Vector2I.Down, speed: 15f);
    }

    private void ShowWaveUI()
    {
        _waveUI?.ShowWave(_currentWave + 1);
    }

    private void OnChildAdded(Node n)
    {
        if (n is not Enemy e) return;

        e.Died -= OnEnemyDied;
        e.Died += OnEnemyDied;

        if (e.CountsForWave)
        {
            _aliveEnemies++;
            GD.Print("Registered enemy. Alive now: ", _aliveEnemies);
        }
    }

    private void OnEnemyDied(Enemy e)
    {
        if (e is Trojan)
            SpawnSplitViruses(e.GlobalPosition);

        if (!e.CountsForWave) return;

        _aliveEnemies--;
        _totalEnemiesKilled++;

        GD.Print("Alive now: ", _aliveEnemies);

        if (_aliveEnemies <= 0 && !_nextWaveScheduled)
        {
            _nextWaveScheduled = true;
            GD.Print("Wave ", _currentWave, " completed");
            GetTree().CreateTimer(1.5).Timeout += StartNextWave;
        }
    }

    private struct WaveInfo
    {
        public int Virus;
        public int Trojan;
        public int Adware;

        public WaveInfo(int v, int t, int a)
        {
            Virus = v;
            Trojan = t;
            Adware = a;
        }
    }

    private WaveInfo GenerateWave(int waveIndex)
    {
        float difficulty = 1f + waveIndex * 0.25f;

        int baseVirus = 2 + (int)(difficulty * 2);
        int baseTrojan = waveIndex >= 2 ? (int)(difficulty * 0.7f) : 0;
        int baseAdware = waveIndex >= 4 ? (int)(difficulty * 0.4f) : 0;

        int virus = GD.RandRange(baseVirus - 1, baseVirus + 2);
        int trojan = GD.RandRange(baseTrojan - 1, baseTrojan + 1);
        int adware = GD.RandRange(baseAdware - 1, baseAdware + 1);

        return new WaveInfo(
            Math.Max(virus, 0),
            Math.Max(trojan, 0),
            Math.Max(adware, 0)
        );
    }

    private void SpawnWave(WaveInfo wave)
    {
        SpawnEnemies(VirusScene, wave.Virus);
        SpawnEnemies(TrojanScene, wave.Trojan);
        SpawnEnemies(AdwareScene, wave.Adware);
    }

    private void SpawnEnemies(PackedScene scene, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var enemy = scene.Instantiate<Node2D>();
            enemy.Position = GetSafeSpawnPosition();
            CallDeferred("add_child", enemy);
        }
    }

    private void SpawnSplitViruses(Vector2 origin)
    {
        int count = GD.RandRange(1, 2);

        for (int i = 0; i < count; i++)
        {
            var virus = (Enemy)VirusScene.Instantiate();
            virus.Position = origin + new Vector2(
                GD.Randf() * 16 - 8,
                GD.Randf() * 16 - 8
            );
            virus.CountsForWave = true;
            CallDeferred("add_child", virus);
        }
    }

    private Vector2 GetSafeSpawnPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            var sp = SpawnPoints[GD.Randi() % SpawnPoints.Length];
            var offset = new Vector2(
                GD.Randf() * 48f - 24f,
                GD.Randf() * 48f - 24f
            );

            var pos = sp.Position + offset;

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

    public int GetTotalEnemiesKilled() => _totalEnemiesKilled;
    public int GetTotalPowerUpsCollected() => _totalPowerUpsCollected;
    public int GetCurrentWaveIndex() => _currentWave;

    public void CollectPowerUp()
    {
        _totalPowerUpsCollected++;
    }
}
