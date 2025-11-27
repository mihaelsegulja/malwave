using Godot;

public partial class GameOverUI : CanvasLayer
{
  private Label _waveReachedLabel;
  private Label _enemiesKilledLabel;
  private Label _powerUpsCollectedLabel;
  private Button _restartButton;
  private Button _quitButton;

  public override void _Ready()
  {
    ProcessMode = Node.ProcessModeEnum.Always;
    var vBox = GetNode<VBoxContainer>("Control/CenterContainer/VBoxContainer");
    _waveReachedLabel = vBox.GetNode<Label>("WaveReached");
    _enemiesKilledLabel = vBox.GetNode<Label>("EnemiesKilled");
    _powerUpsCollectedLabel = vBox.GetNode<Label>("PowerUpsCollected");
    _restartButton = vBox.GetNode<Button>("Restart");
    _quitButton = vBox.GetNode<Button>("Quit");

    _restartButton.Pressed += OnRestart;
    _quitButton.Pressed += OnQuit;
  }

  public void ShowGameOver(int waveReached, int enemiesKilled, int powerUpsCollected)
  {
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
