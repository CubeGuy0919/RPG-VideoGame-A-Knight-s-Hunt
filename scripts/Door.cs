using Godot;
using System;

public partial class Door : Area2D
{

    private bool playerNearby = false;

    private Player playerRef;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body){
        if(body is Player player){
            playerNearby = true;
            playerRef = player;
        }   
    }

    private void OnBodyExited(Node body){
        if(body is Player player){
            playerNearby = false;
            playerRef = null;
        }   
    }

    public override void _Process(double delta){
        if(playerNearby && Input.IsActionPressed("ui_accept"))
        {
            if(playerRef.HasKey)
            {
                QueueFree();
                GD.Print("Door opened");
            }
            else{
                GD.Print("No key");
            }

        
        }
    }


}
