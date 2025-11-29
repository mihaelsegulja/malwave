using Godot;

public partial class GameOverUI : CanvasLayer
{
    private Label _waveReachedLabel;
    private Label _enemiesKilledLabel;
    private Label _powerUpsCollectedLabel;

    private Button _restartButton;
    private Button _quitButton;

    private AudioStreamPlayer _gameOverSfx;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        var vbox = GetNode<VBoxContainer>("Control/CenterContainer/VBoxContainer");

        _waveReachedLabel = vbox.GetNode<Label>("WaveReached");
        _enemiesKilledLabel = vbox.GetNode<Label>("EnemiesKilled");
        _powerUpsCollectedLabel = vbox.GetNode<Label>("PowerUpsCollected");

        _restartButton = vbox.GetNode<Button>("Restart");
        _quitButton = vbox.GetNode<Button>("Quit");

        _gameOverSfx = GetNodeOrNull<AudioStreamPlayer>("GameOverSFX");

        _restartButton.Pressed += OnRestart;
        _quitButton.Pressed += OnQuit;
    }

    public void ShowGameOver(int waveReached, int enemiesKilled, int powerUpsCollected)
    {
        _gameOverSfx?.Play();

        _waveReachedLabel.Text = $"Wave reached: {waveReached}";
        _enemiesKilledLabel.Text = $"Malware killed: {enemiesKilled}";
        _powerUpsCollectedLabel.Text = $"PowerUps collected: {powerUpsCollected}";

        GetTree().Paused = true;
    }

    private void OnRestart()
    {
        GetTree().Paused = false;
        QueueFree();
        GetTree().ChangeSceneToFile("res://src/Scenes/Main.tscn");
    }

    private void OnQuit()
    {
        GetTree().Quit();
    }
}
