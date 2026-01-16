using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueManager : CanvasLayer
{
	private RichTextLabel _dialogText;
	private Label _speakerLabel;
	private Control _nextIndicator;
	private Panel _panel;

	private List<string> _dialogueLines = new List<string>();
	private int _currentLineIndex = 0;
	private bool _isActive = false;
	private bool _isTyping = false;

	[Export] public float TextSpeed = 0.05f; 
	private double _timer = 0;

	public override void _Ready()
	{
		// These paths must match Image 5 exactly
		_panel = GetNode<Panel>("CanvasLayer/Panel");
		_dialogText = GetNode<RichTextLabel>("CanvasLayer/Panel/MarginContainer/DialogText");
		_speakerLabel = GetNode<Label>("CanvasLayer/Panel/MarginContainer/DialogSpeaker");
		_nextIndicator = GetNode<Control>("CanvasLayer/Panel/NextIndicator");

		// Hide the panel at the very start so it isn't seen
		_panel.Visible = false;
		_nextIndicator.Hide();

		// Ensure the UI works even when the game is paused
		ProcessMode = ProcessModeEnum.Always;
		GD.Print("DialogueManager Ready");
	}		

	public void StartDialogue(string speakerName, string[] lines)
	{
		_speakerLabel.Text = speakerName;
		_dialogueLines = new List<string>(lines);
		_currentLineIndex = 0;
		_isActive = true;
		
		_panel.Visible = true; // Show the panel now!
		
		GetTree().Paused = true; // This freezes the player
		StartLine();
	}

	private void StartLine()
	{
		_dialogText.Text = _dialogueLines[_currentLineIndex];
		_dialogText.VisibleCharacters = 0; 
		_nextIndicator.Hide();
		_isTyping = true;
		_timer = 0;
	}

	public override void _Process(double delta)
	{
		if (_isActive && _isTyping)
		{
			_timer += delta;
			if (_timer >= TextSpeed)
			{
				_timer = 0;
				_dialogText.VisibleCharacters++;

				if (_dialogText.VisibleCharacters >= _dialogText.Text.Length)
				{
					OnLineFinished();
				}
			}
		}
	}

	private void OnLineFinished()
	{
		_isTyping = false;
		_nextIndicator.Show();
		GetNode<AnimationPlayer>("AnimationPlayer").Play("bounce");
	}

	public override void _Input(InputEvent @event)
	{
		if (_isActive && @event.IsActionPressed("ui_accept"))
		{
			if (_isTyping)
			{
				_dialogText.VisibleCharacters = _dialogText.Text.Length;
				OnLineFinished();
			}
			else 
			{
				_currentLineIndex++;
				if (_currentLineIndex < _dialogueLines.Count)
				{
					StartLine();
				}
				else
				{
					ExitDialogue();
				}
			}
		}
	}

	private void ExitDialogue()
	{
		_isActive = false;
		_panel.Visible = false;
		_nextIndicator.Hide();
		GetTree().Paused = false;
		
		var npc = GetTree().Root.FindChild("Npc", true, false) as Npc;
		if (npc != null) npc.EndDialogue();
	}
}
