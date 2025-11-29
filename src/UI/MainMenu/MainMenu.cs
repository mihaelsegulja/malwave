using Godot;

public partial class MainMenu : CanvasLayer
{
    [Export] public bool IsPauseMenu { get; set; } = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        var startButton = GetNode<Button>("Control/MarginContainer/VBoxContainer/Start");
        var quitButton = GetNode<Button>("Control/MarginContainer/VBoxContainer/Quit");

        startButton.Text = IsPauseMenu ? "Continue" : "Start";

        startButton.Pressed += OnStart;
        quitButton.Pressed += OnQuit;
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
