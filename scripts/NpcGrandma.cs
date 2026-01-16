using Godot;
using System;

public partial class NpcGrandma : CharacterBody2D
{
    [Export] public string[] DialogueLines = { 
        "Greetings, traveler!", 
        "I hope you are enjoying the typewriter effect!", 
        "Goodbye for now." 
    };

    [Export] public Resource DialogueResource;
    [Export] public string DialogueStart = "start";

    [Export] public Player playerNode; 

    private AnimatedSprite2D _sprite;
    private bool _playerNearby = false;
    private bool _isTalking = false; 


    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite.Play("idle");

        Area2D area = GetNode<Area2D>("Actionable");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
        
        // Connect to the addon's signal to know when to stop animating
        Node dm = GetNode("/root/DialogueManager");
        dm.Connect("dialogue_ended", Callable.From((Resource res) => EndDialogue()));
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.Name == "player" || body is Player) 
        {
            _playerNearby = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.Name == "player" || body is Player)
        {
            _playerNearby = false;
            
            // If the player leaves while NOT talking, go back to standard idle
            if (!_isTalking)
            {
                _sprite.Play("idle");
            }
        }
    }

    public override void _Process(double delta)
    {
        // Only allow interaction if nearby and not already talking
        if (_playerNearby && !_isTalking && Input.IsActionJustPressed("ui_accept"))
        {
            Talk();
        }
    }

private void Talk()
{
    if (DialogueResource == null)
    {
        GD.Print("ERROR: No Dialogue Resource assigned to NPC!");
        return;
    }

    _isTalking = true;
    _sprite.Play("talking");

    // Get the Autoloaded Singleton
    Node dm = GetNode("/root/DialogueManager");

    // Log for debugging to make sure the resource path is correct
    GD.Print("Resource Path: " + DialogueResource.ResourcePath);

    // Call 'show_dialogue_balloon' (this is the standard method in the addon)
    // Argument 1: The Resource file
    // Argument 2: The Title string ("start")
    dm.Call("show_dialogue_balloon", DialogueResource, DialogueStart);
}   

    public void EndDialogue()
    {
        _isTalking = false;
        
        _sprite.Play("idle");

        playerNode.WinGame();
    }
}