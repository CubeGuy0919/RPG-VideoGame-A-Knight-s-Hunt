using Godot;
using System;
using DialogueManagerRuntime;

public partial class Actionable : Area2D
{
    [Export] public Resource DialogueResource;

    [Export] public string DialogueStart = "start";

    public void Action()
    {
        // 1. Get the DialogueManager Singleton node from the root
        // This matches the "DialogueManager" name registered in your GDScript file
        var dialogueManager = GetNode("/root/DialogueManager");

        // 2. Use .Call to trigger the GDScript method 'show_dialogue_balloon' 
        // Note: GDScript uses snake_case, so use "show_dialogue_balloon"
        dialogueManager.Call("show_dialogue_balloon", DialogueResource, DialogueStart);
    }
}
