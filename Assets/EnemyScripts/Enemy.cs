using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
    [Tooltip("What element is current enemy")]
    [SerializeField] private string EnemyTypes;

    [Tooltip("Material to switch to during the flash.")]
    [SerializeField] private Material flashMaterial;

    [Tooltip("Duration of the flash.")]
    [SerializeField] private float duration;

    // The SpriteRenderer that should flash.
    private SpriteRenderer spriteRenderer;

    // The material that was in use, when the script started.
    private Material originalMaterial;

    // The currently running coroutine.
    private Coroutine flashRoutine;

    public int hp = 30;
    //
    AudioManager audioManager;

    private Rigidbody2D EnemyRb;
    [SerializeField] private float Knockbackforce = 0.05f;
    //[SerializeField] private float knockbackLockout = 0.20f;
    [SerializeField] private float knockbackLockout = 0.30f; // was 0.20f

    [Header("Return/Home Settings")]
                 // not used directly anymore, kept for reference
    [SerializeField] private float homeAccel = 12f;               // how hard to pull back to start
    [SerializeField] private float homeStopRadius = 0.05f;        // snap/stop distance
    //[SerializeField] private float maxSpeed = 6f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("HitStop Setting")]
    [SerializeField] private float HitStopDuration = 5f;

    [Header("HP (moved out of Enemy logic)")]
    [SerializeField] private Health health;   // assign in Inspector or via GetComponent in Awake
    [Header("Element")]
    public Element defenseElement = Element.Neutral; // optional mirror for convenience


    // NEW: track spawn position
    private Vector2 startPosition;

    private float knockbackUntilTime = -1f;

    [SerializeField] private float maxSpeed = 6f;          // normal cap
    [SerializeField] private float knockbackMaxSpeed = 20f; // higher cap just during knockback
    public bool InKnockback => Time.time < knockbackUntilTime;
   



    [SerializeField] public Animator EnemyAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the SpriteRenderer to be used,
        // alternatively you could set it from the inspector.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the material that the SpriteRenderer uses, 
        // so we can switch back to it after the flash ended.
        originalMaterial = spriteRenderer.material;
        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();
        EnemyRb = GetComponent<Rigidbody2D>();

        // record where the enemy spawned
        startPosition = transform.position;
        // Physics safety
        EnemyRb.freezeRotation = true;
        EnemyRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        EnemyRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        EnemyRb.gravityScale = 0f;

        // Give it some drag so impulses die out naturally
        //EnemyRb.linearDamping = 4f;          // tune to taste (higher = stops quicker)
        EnemyRb.linearDamping = 1.0f;   // was 4f; try 0.5–1.5 for nicer shove persistence
        EnemyRb.angularDamping = 0.05f;

        //EnemyRb.drag = 4f;            // instead of linearDamping
        //EnemyRb.angularDrag = 0.05f;
    }
    private void Awake()
    {
        // Keep your current Awake content
        if (EnemyRb == null) EnemyRb = GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<Health>();
        if (health != null) health.defenseElement = defenseElement;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //ApplyReturnToStartPhysics();
        // Optional: hard cap speed to avoid silly values
        float cap = InKnockback ? knockbackMaxSpeed : maxSpeed;
        EnemyRb.linearVelocity = Vector2.ClampMagnitude(EnemyRb.linearVelocity, cap);
        //EnemyRb.linearVelocity = Vector2.ClampMagnitude(EnemyRb.linearVelocity, maxSpeed);
    }

    //public void Damaged(string AbilityElement, Transform playertransform)
    //{
    //    Debug.Log("PASSED IN : "+AbilityElement);

        //if (EnemyTypes == "Water")
        //{
        //    EnemyAnimator.SetTrigger("WaterHit");
        //}else if(EnemyTypes == "Fire")
        //{
        //    EnemyAnimator.SetTrigger("FireHit");
        //}
    //    audioManager.PlaySFX(audioManager.HitSFX);
    //    audioManager.PlaySFX(audioManager.EnemyScreamSFX);
    //    Knockback(playertransform);
    //    Timestop();
    //    Flash();



    //    hp--;


    //}

    // ======= NEW: preferred overload =======
    public void Damaged(DamagePacket packet)
    {
        if (health == null)
        {
            // Fallback: just do your old behavior if Health missing
            // (but you should add Health to all enemies)
            ApplyKnockback(packet.knockbackDir, packet.knockbackForce * 0.5f);
            return;
        }

        bool didDamage = health.ApplyDamage(packet, out float finalDamage);

        if (didDamage)
        {
            // >>> Your existing reactions for a "real hit":
            // - Knockback (use packet.knockbackDir/Force)
            // - Optional flash (COMMENT OUT if you want no flash globally for now)
            // - Hitstop
            audioManager.PlaySFX(audioManager.HitSFX);
            if (defenseElement == Element.Water)
            {
                audioManager.PlaySFX(audioManager.WaterDemonDamage);
                EnemyAnimator.SetTrigger("WaterHit");
            }
            else if (defenseElement == Element.Fire)
            {
                audioManager.PlaySFX(audioManager.EnemyScreamSFX);
                EnemyAnimator.SetTrigger("FireHit");
            }
            ApplyKnockback(packet.knockbackDir, packet.knockbackForce);
            Flash();
            
           
            
            // HITFLASH: comment if you want to disable globally for now
            // StartCoroutine(DoFlash());  // <-- your existing flash routine

            //DoHitstop(); // your existing hitstop routine
        }
        else
        {
            // >>> IMMUNE FEEL: small knockback, no flash, no hitstop
            ApplyKnockback(packet.knockbackDir, packet.knockbackForce * 0.4f);
        }
    }

    // ======= BACK-COMPAT: old signature wrapper =======
    // If other code still calls Enemy.Damaged(string elementName, Transform player)
    public void OldDamaged(string incomingElementName, Transform playerTransform)
    {
        // Map your old string to our enum (fallback to Neutral if unknown)
        Element atkElem = incomingElementName switch
        {
            "Fire" => Element.Fire,
            "Water" => Element.Water,
            "Wind" => Element.Wind,
            _ => Element.Neutral
        };

        Vector2 dir = (transform.position - playerTransform.position).normalized;
        float force = 4.0f; // use your existing constant/serialized value if you have one

        // Base damage: for old path, use 1 or your Hitbox.damage if available
        float baseDmg = 1f;

        var pkt = new DamagePacket(baseDmg, atkElem, playerTransform, dir, force);
        Damaged(pkt);
    }

    //public void Knockback(Transform playertransform)
    //{
      
    //    Debug.Log("ENEMY KNOCKED BACK");

    //    Vector2 dir = ((Vector2)transform.position - (Vector2)playertransform.position).normalized;
    //    if (dir == Vector2.zero) dir = Vector2.right;

    //    // Clear existing velocity to avoid stacking
    //    EnemyRb.linearVelocity = Vector2.zero;

    //    // Instant shove
    //    EnemyRb.AddForce(dir * Knockbackforce, ForceMode2D.Impulse);

    //    // Put enemy in a brief "don't home back yet" window
    //    knockbackUntilTime = Time.time + knockbackLockout;
    //}
    private void ApplyKnockback(Vector2 dir, float force)
    {
        //// your current knockback code
        //if (EnemyRb == null) return;

        //// Safety: normalize and handle degenerate vectors
        //if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        //dir = dir.normalized;

        //// Clear current motion so impulses feel crisp (prevents “ice skating”)
        //// If you're on an older Unity version, use: EnemyRb.velocity instead of linearVelocity
        //EnemyRb.linearVelocity = Vector2.zero;

        //// Instant shove away from the source
        //EnemyRb.AddForce(dir * Mathf.Max(0f, force), ForceMode2D.Impulse);

        //// Brief lockout so homing/other forces don’t immediately cancel the knockback
        //knockbackUntilTime = Time.time + knockbackLockout;

        //// Optional: keep speeds sane in case force is very high
        //EnemyRb.linearVelocity = Vector2.ClampMagnitude(EnemyRb.linearVelocity, maxSpeed);

        if (EnemyRb == null) return;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir = dir.normalized;

        EnemyRb.linearVelocity = Vector2.zero;                               // clear motion
        EnemyRb.AddForce(dir * Mathf.Max(0f, force), ForceMode2D.Impulse);

        knockbackUntilTime = Time.time + knockbackLockout;

        // Optional cap
        //EnemyRb.linearVelocity = Vector2.ClampMagnitude(EnemyRb.linearVelocity, maxSpeed);
    }

    private void DoHitstop()
    {
        // your current timescale hitstop code (WaitStop coroutine etc.)
        Timestop();
    }
    void Flash()
    {
        // If the flashRoutine is not null, then it is currently running.
        if (flashRoutine != null)
        {
            // In this case, we should stop it first.
            // Multiple FlashRoutines the same time would cause bugs.
            StopCoroutine(flashRoutine);
        }

        // Start the Coroutine, and store the reference for it.
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Swap to the flashMaterial.
        spriteRenderer.material = flashMaterial;

        // Pause the execution of this function for "duration" seconds.
        yield return new WaitForSeconds(duration);

        // After the pause, swap back to the original material.
        spriteRenderer.material = originalMaterial;

        // Set the routine to null, signaling that it's finished.
        flashRoutine = null;
    }
    // -------------------------------
    // NEW FUNCTION: Enemy moves back to spawn
    // -------------------------------
    private void ReturnToStart()
    {
        // if close enough, stop moving
        if (Vector2.Distance(transform.position, startPosition) > 2f)
        {
            // Move enemy gradually back
            Vector2 newPos = Vector2.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            EnemyRb.MovePosition(newPos);
        }
    }

    // NEW: physics-based homing back to spawn (no MovePosition; runs in FixedUpdate)
    private void ApplyReturnToStartPhysics()
    {
        // During knockback lockout, do not home back
        if (Time.time < knockbackUntilTime) return;

        Vector2 pos = EnemyRb.position;
        Vector2 toStart = startPosition - pos;
        float dist = toStart.magnitude;

        if (dist <= homeStopRadius)
        {
            // close enough �� stop drift and snap gently
            EnemyRb.linearVelocity = Vector2.zero;
            EnemyRb.position = startPosition; // safe small snap
            return;
        }

        // Apply a gentle steering force back toward start
        Vector2 desiredDir = toStart / dist;
        EnemyRb.AddForce(desiredDir * homeAccel, ForceMode2D.Force);
    }

    bool waiting;
    public void Timestop()
    {
        if (waiting) { return; }
        Time.timeScale = 0.0f;
        StartCoroutine(WaitStop());

    }

    IEnumerator WaitStop()
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(HitStopDuration);
        Time.timeScale = 1.0f;
        waiting = false;
    }

}
