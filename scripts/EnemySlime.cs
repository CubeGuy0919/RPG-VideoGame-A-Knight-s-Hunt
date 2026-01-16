using Godot;

public partial class EnemySlime : CharacterBody2D
{
    [Export] public float MoveSpeed = 25f;
    [Export] public int Damage = 10;

    private Player player;
    private bool playerInDetectionRange = false;
    private bool playerInAttackRange = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isDead = false;

    private AnimatedSprite2D sprite;
    private Timer attackCooldown;
    private HitBox slimeHitbox;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        attackCooldown = GetNode<Timer>("AttackCooldown");
        slimeHitbox = GetNode<HitBox>("Hitbox"); // Make sure Slime has a Hitbox child

        slimeHitbox.Monitoring = false;
        slimeHitbox.Damage = Damage;

        GetNode<Area2D>("Detection_Area").BodyEntered += OnDetectionBodyEntered;
        GetNode<Area2D>("Detection_Area").BodyExited += OnDetectionBodyExited;

        GetNode<Area2D>("Attack_Area").BodyEntered += OnAttackBodyEntered;
        GetNode<Area2D>("Attack_Area").BodyExited += OnAttackBodyExited;

        attackCooldown.Timeout += OnAttackCooldownTimeout;
        sprite.AnimationFinished += OnAnimationFinished;
        sprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDead) return;

        if (isAttacking)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        if (playerInDetectionRange && player != null)
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * MoveSpeed;
            PlayWalkAnimation(direction);
        }
        else
        {
            Velocity = Vector2.Zero;
            if (sprite.Animation != "idle") sprite.Play("idle");
        }

        MoveAndSlide();

        if (playerInAttackRange && canAttack && !isAttacking)
            StartAttack();
    }
    private void StartAttack()
    {
        canAttack = false;
        isAttacking = true;
        Velocity = Vector2.Zero;
        
        // 1. Turn on the hitbox monitoring
        slimeHitbox.Monitoring = true; 

        // 2. NEW: Manually check for the player right now
        // This looks for any HurtBoxes already inside the slime's attack radius
        foreach (Area2D area in slimeHitbox.GetOverlappingAreas())
        {
            if (area is HurtBox hb)
            {
                GD.Print("[SLIME] Found player! Dealing damage now.");
                hb.ReceiveHit(Damage);
            }
        }

        sprite.Play("attack");
}

   private void OnAnimationFinished()
{
    if (sprite.Animation == "death")
    {
        QueueFree(); // Only disappear AFTER the animation
    }
    else if (sprite.Animation == "hurt")
    {
        // Return to normal behavior
        sprite.Play("idle");
    }
    else if (sprite.Animation == "attack")
{
    isAttacking = false;
    slimeHitbox.Monitoring = false; 
    attackCooldown.Start();
}
}

    private void PlayWalkAnimation(Vector2 direction)
    {
        if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
            sprite.Play(direction.X > 0 ? "walk_right" : "walk_left");
        else
            sprite.Play(direction.Y > 0 ? "walk_down" : "walk_up");
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        
        isAttacking = false; // Interrupt attack if hit
        sprite.Play("hurt");
        GD.Print("[SLIME] Playing Hurt Animation");
    }

public void Die()
{
    isDead = true;
    Velocity = Vector2.Zero;
    sprite.Play("death");
    GD.Print("[SLIME] Playing Death Animation");
    GD.Print("[SLIME] Dead");
}

    private void OnAttackCooldownTimeout() { canAttack = true; }
    private void OnDetectionBodyEntered(Node body) { if (body is Player p) { player = p; playerInDetectionRange = true; } }
    private void OnDetectionBodyExited(Node body) { if (body is Player) { player = null; playerInDetectionRange = false; } }
    private void OnAttackBodyEntered(Node body) { if (body is Player) playerInAttackRange = true; }
    private void OnAttackBodyExited(Node body) { if (body is Player) playerInAttackRange = false; }
}