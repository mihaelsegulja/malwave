using Godot;
using System.Collections.Generic;

public partial class ArenaManager : Node2D
{
    [Export] public float PulseInterval = 0.2f;
    [Export] public float GlowDuration = 0.5f;

    private readonly List<int> GlowTileIDs = new List<int> { 1, 3, 5, 7 };
    private const int NonGlowTileID = 8;
    private const int WallTileID = 9;
    private TileMapLayer _tileMapLayer;
    private const int SourceID = 1;
    private const int AtlasYCoord = 0;
    private bool _waveActive = false;
    private float _waveTime = 0f;
    private Vector2I _waveStart;
    private float _waveSpeed = 20f;
    private HashSet<Vector2I> _waveHit = new();
    private List<Vector2I> _allTiles = new();

    public override void _Ready()
    {
        _tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");
        CacheTiles();
        StartAutomaticPulsing();
    }

    private void CacheTiles()
    {
        _allTiles.Clear();
        var used = _tileMapLayer.GetUsedCells();

        foreach (var pos in used)
        {
            var atlas = _tileMapLayer.GetCellAtlasCoords(pos);
            if (atlas.X != WallTileID)
                _allTiles.Add(pos);
        }
    }

    private void StartAutomaticPulsing()
    {
        var timer = GetTree().CreateTimer(PulseInterval, false);
        timer.Timeout += () =>
        {
            PulseRandomTile();
            StartAutomaticPulsing();
        };
    }

    private void PulseRandomTile()
    {
        List<Vector2I> valid = new();

        foreach (var tilePos in _allTiles)
        {
            var atlas = _tileMapLayer.GetCellAtlasCoords(tilePos);

            if (atlas.X == NonGlowTileID)
                valid.Add(tilePos);
        }

        if (valid.Count > 0)
        {
            int idx = (int)GD.RandRange(0, valid.Count - 1);
            TurnTileOn(valid[idx], GlowDuration);
        }
    }

    private void TurnTileOn(Vector2I tilePos, float duration)
    {
        int glowID = GetRandomGlowTileID();

        _tileMapLayer.SetCell(tilePos, SourceID, new Vector2I(glowID, AtlasYCoord), 0);

        var timer = GetTree().CreateTimer(duration, false);
        timer.Timeout += () =>
        {
            if (!IsInstanceValid(_tileMapLayer)) return;
            
            var atlas = _tileMapLayer.GetCellAtlasCoords(tilePos);

            if (atlas.X != NonGlowTileID) 
                _tileMapLayer.SetCell(tilePos, SourceID, new Vector2I(NonGlowTileID, AtlasYCoord), 0);
        };
    }

    public void FlashTileOnEvent(Vector2 worldPosition, float duration = 0.5f)
    {
        Vector2I tilePos = _tileMapLayer.LocalToMap(_tileMapLayer.ToLocal(worldPosition));

        var atlas = _tileMapLayer.GetCellAtlasCoords(tilePos);

        if (atlas.X == NonGlowTileID)
            TurnTileOn(tilePos, duration);
    }

    private int GetRandomGlowTileID()
    {
        int index = (int)GD.RandRange(0, GlowTileIDs.Count - 1);
        return GlowTileIDs[index];
    }

    public void StartWaveSweep(Vector2I startTile, Vector2I direction, float speed = 20f)
    {
        _waveStart = startTile;
        _waveSpeed = speed;
        _waveTime = 0f;
        _waveHit.Clear();
        _waveDirection = direction;
        _waveActive = true;
    }

    private Vector2I _waveDirection = Vector2I.Right;

    public override void _Process(double delta)
    {
        if (!_waveActive)
            return;

        _waveTime += (float)delta;

        float wavePos = _waveTime * _waveSpeed;

        foreach (var tile in _allTiles)
        {
            if (_waveHit.Contains(tile)) 
                continue;

            Vector2 rel = tile - _waveStart;
            float dist = rel.Dot(_waveDirection);

            if (dist <= wavePos && dist >= 0)
            {
                _waveHit.Add(tile);
                TurnTileOn(tile, GlowDuration * 0.8f);
            }
        }

        if (_waveHit.Count >= _allTiles.Count)
            _waveActive = false;
    }
}
