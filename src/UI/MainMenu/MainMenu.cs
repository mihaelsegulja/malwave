using Godot;

public partial class MainMenu : CanvasLayer
{
  [Export] public bool IsPauseMenu = false;

  public override void _Ready()
  {
    ProcessMode = Node.ProcessModeEnum.Always;
    var startBtn = GetNode<Button>("Control/MarginContainer/VBoxContainer/Start");
    var quitBtn = GetNode<Button>("Control/MarginContainer/VBoxContainer/Quit");

    startBtn.Text = !IsPauseMenu ? "Start" : "Continue";

    startBtn.Pressed += OnStart;
    quitBtn.Pressed += OnQuit;
  }

  private void OnStart()
  {
    if (IsPauseMenu)
    {
      GetTree().Paused = false;
      QueueFree();
      return;
    }

    GetTree().ChangeSceneToFile("res://src/Scenes/Main.tscn");
  }

  private void OnQuit()
  {
    GetTree().Quit();
  }
}
