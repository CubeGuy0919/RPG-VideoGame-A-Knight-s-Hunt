using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float MoveSpeed = 50f;
    [Export] public int MaxHP = 100;
    [Export] public int AttackDamage = 30;
    [Export] private Node2D ActionableFinder;
    [Export] public Control GameOverUI;
    [Export] public ColorRect Vignette;

    private AnimatedSprite2D sprite;
    private Area2D hitbox;

    private enum PlayerState { Idle, Move, Attack, Hurt, Dead }
    private PlayerState state = PlayerState.Idle;
    private float stateTimer = 0f;
    private Vector2 facingDirection = Vector2.Right;
    private int currentHP;

    public bool HasKey { get; private set; } = false;

    public override void _Ready()
    {
        GlobalPosition = new Vector2(-40, 290); //-40
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        hitbox = GetNode<Area2D>("Hitbox");

        hitbox.Monitoring = false;
        currentHP = MaxHP;

        sprite.AnimationFinished += OnAnimationFinished;
    }

    public override void _Input(InputEvent e)
    {
        if (state == PlayerState.Hurt) return;

        // --- RESTART LOGIC ---
        if (state == PlayerState.Dead && e.IsActionPressed("Restart"))
        {
            ShowGameOverScreen();
            //RestartGame();
            return;
        }

        if (e.IsActionPressed("Attack") && state != PlayerState.Attack)
        {
            EnterAttack();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().Paused) 
        {
            Velocity = Vector2.Zero;
            if (sprite.Animation != "death") sprite.Play("death");
            return;
        }

        if (state == PlayerState.Attack || state == PlayerState.Hurt)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        if (state == PlayerState.Dead) return;

        HandleMovement();
        MoveAndSlide();
    }

    private void EnterAttack()
    {
        GD.Print("[PLAYER] Attack Started");
        state = PlayerState.Attack;

        UpdateHitboxDirection();

        if (hitbox is HitBox playerHitBox)
        {
            playerHitBox.Damage = AttackDamage; 
        }

        hitbox.Monitoring = true; 
        
        if (hitbox is HitBox hb) hb.CheckForOverlap(); 

        sprite.FlipH = facingDirection.X < 0;
        sprite.Play("attack"); 
    }

    private void OnAnimationFinished()
    {
        if (sprite.Animation == "death")
        {
            GD.Print("[PLAYER] Death animation finished. Waiting for 'R' to restart.");
        }
        else if (sprite.Animation == "hurt" || sprite.Animation == "attack")
        {
            ExitState();
        }
    }

    private void ShowGameOverScreen()
    {
        GD.Print("[GAME OVER] Restarting in 3 seconds...");
        
        // Restart Delay
        var timer = GetTree().CreateTimer(1.0); 
        timer.Timeout += () => {
        if (state == PlayerState.Dead) RestartGame();
    };
    }

    private void ExitState()
    {
        hitbox.Monitoring = false;
        state = PlayerState.Idle;
        GD.Print("[PLAYER] Back to Idle");
    }

    private void HandleMovement()
    {
        Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = dir * MoveSpeed;

        if (dir.X != 0) facingDirection = new Vector2(dir.X, 0);

        if (dir == Vector2.Zero)
        {
            state = PlayerState.Idle;
            if (sprite.Animation != "idle") sprite.Play("idle");
        }
        else
        {
            state = PlayerState.Move;
            PlayWalkAnimation(dir);
        }
    }

    private void PlayWalkAnimation(Vector2 dir)
    {
        if (dir.X != 0)
        {
            sprite.FlipH = dir.X < 0;
            sprite.Play("walk_right");
        }
        else
        {
            sprite.Play(dir.Y > 0 ? "walk_down" : "walk_up");
        }
    }

    private void UpdateHitboxDirection()
    {
        hitbox.Position = facingDirection.X > 0 ? new Vector2(16, 0) : new Vector2(-16, 0);
    }

    public void TakeDamage(int amount)
    {
        if (state == PlayerState.Dead) return;
        
        state = PlayerState.Hurt;
        Velocity = Vector2.Zero; 
        sprite.Play("hurt");
        GD.Print("[PLAYER] Playing Hurt Animation");
    }

    public void Die()
    {
        state = PlayerState.Dead;
        Velocity = Vector2.Zero;
        hitbox.Monitoring = false;
        sprite.Play("death");
        GD.Print("[PLAYER] Playing Death Animation");
        if (GameOverUI != null)
        {
            // Prepare UI for Fade
            GameOverUI.Modulate = new Color(1, 1, 1, 0); 
            GameOverUI.Visible = true;
            GD.Print("[PLAYER] Showing Game Over");

            if (Vignette != null)
            {
                Vignette.Modulate = new Color(1, 1, 1, 0);
                Vignette.Visible = true;
            }

            // Create the Smooth Fade-In Tween
            Tween fadeTween = GetTree().CreateTween().SetParallel(true);
            fadeTween.TweenProperty(GameOverUI, "modulate:a", 1.0f, 1.5f);
            
            if (Vignette != null)
            {
                fadeTween.TweenProperty(Vignette, "modulate:a", 1.0f, 1.0f);
            }
        }
        else
        {
            GD.Print("[PLAYER] NOT Showing Game Over");
        }
        GD.Print("[PLAYER] Dead");
    }

    private void RestartGame()
    {
        GD.Print("[GAME] Restarting...");
        GlobalPosition = new Vector2(40, 290);
        GetTree().Paused = false; 
        GetTree().ReloadCurrentScene();
    }

    public void PickupKey() { HasKey = true; }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            var actionables = ((Area2D)ActionableFinder).GetOverlappingAreas();
            if (actionables.Count > 0)
            {
                if (actionables[0] is Actionable action)
                {
                    action.Action();
                }
            }
        }
    }

    [Export] public Control VictoryUI; 

    public void WinGame()
    {
        state = PlayerState.Idle; 
        Velocity = Vector2.Zero;
        GetTree().Paused = true;

        if (VictoryUI != null)
        {
            VictoryUI.Visible = true;
            VictoryUI.Modulate = new Color(1, 1, 1, 0);
            
            Tween winTween = GetTree().CreateTween();
            winTween.TweenProperty(VictoryUI, "modulate:a", 1.0f, 2.0f);
        }
        GD.Print("[GAME] Grandma says you won!");
    }
}