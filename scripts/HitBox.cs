using Godot;

public partial class HitBox : Area2D
{
    [Export] public int Damage = 10;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        GD.Print("[HITBOX] Something entered the attack area");

        if (area is HurtBox hurtbox)
        {
            GD.Print($"[HITBOX] Valid Hurtbox found on {area.Owner.Name}!");
            hurtbox.ReceiveHit(Damage);
        }
    }

    public void CheckForOverlap()
    {
        foreach (Area2D area in GetOverlappingAreas())
        {
            OnAreaEntered(area);
        }
    }
}