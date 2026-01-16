using Godot;

public partial class Health : Node
{
    [Export] public int MaxHP = 100;
    public int CurrentHP { get; private set; }

    public override void _Ready()
    {
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        
        // DEBUG: damage tracking
        GD.Print($"[DEBUG] {Owner.Name} took {amount} damage. Remaining HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hurt state on the character script
            if (Owner.HasMethod("TakeDamage"))
            {
                Owner.Call("TakeDamage", amount);
            }
        }
    }

    private void Die()
    {
        GD.Print($"[DEBUG] {Owner.Name} HP reached 0. Triggering Die logic.");
        
        if (Owner.HasMethod("Die"))
        {
            Owner.Call("Die");
        }
        else
        {
            Owner.QueueFree();
        }
    }
}