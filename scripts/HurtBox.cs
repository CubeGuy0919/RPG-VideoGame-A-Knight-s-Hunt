using Godot;

public partial class HurtBox : Area2D
{
    // In the Inspector, drag your 'Health' node into this slot
    [Export] public Health HealthNode;

    public void ReceiveHit(int damage)
    {
        GD.Print($"[HURTBOX] Received {damage} damage");
        
        // Check if there is a Health node as a child of the owner
        var health = GetOwner().GetNodeOrNull<Health>("Health");
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else if (GetOwner().HasMethod("TakeDamage"))
        {
            // Fallback: call TakeDamage directly on Player.cs or EnemySlime.cs
            GetOwner().Call("TakeDamage", damage);
        }
    }
}
