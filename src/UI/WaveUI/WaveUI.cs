using Godot;
using System;

public partial class WaveUI : CanvasLayer
{
  private Label _waveLabel;
  private Tween _tween;

  public override void _Ready()
  {
    _waveLabel = GetNode<Label>("CenterContainer/Label");

    _waveLabel.Modulate = new Color(1, 1, 1, 0);
  }

  public void ShowWave(int waveNumber)
  {
    if (_waveLabel == null) return;

    _waveLabel.Text = "WAVE " + waveNumber;

    if (_tween != null && _tween.IsValid())
    {
      _tween.Kill();
    }

    _tween = CreateTween();
    _tween.TweenProperty(_waveLabel, "modulate", new Color(1, 1, 1, 1), 0.25);
    _tween.TweenInterval(1.0);
    _tween.TweenProperty(_waveLabel, "modulate", new Color(1, 1, 1, 0), 1.0);
  }
}
