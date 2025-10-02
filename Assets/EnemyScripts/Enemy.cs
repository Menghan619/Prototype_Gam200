using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
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
    [SerializeField] private float knockbackLockout = 0.20f;

    [Header("Return/Home Settings")]
                 // not used directly anymore, kept for reference
    [SerializeField] private float homeAccel = 12f;               // how hard to pull back to start
    [SerializeField] private float homeStopRadius = 0.05f;        // snap/stop distance
    [SerializeField] private float maxSpeed = 6f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("HitStop Setting")]
    [SerializeField] private float HitStopDuration = 5f;

    // NEW: track spawn position
    private Vector2 startPosition;

    private float knockbackUntilTime = -1f;




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
        EnemyRb.linearDamping = 4f;          // tune to taste (higher = stops quicker)
        EnemyRb.angularDamping = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        ApplyReturnToStartPhysics();
        // Optional: hard cap speed to avoid silly values
        EnemyRb.linearVelocity = Vector2.ClampMagnitude(EnemyRb.linearVelocity, maxSpeed);
    }

    public void Damaged(string EnemyType, Transform playertransform)
    {
        Debug.Log("PASSED IN : "+EnemyType);
        if (EnemyType == "EnemyF")
        {
            EnemyAnimator.SetTrigger("WaterHit");
        }else if(EnemyType == "EnemyW")
        {
            EnemyAnimator.SetTrigger("FireHit");
        }
        audioManager.PlaySFX(audioManager.HitSFX);
        audioManager.PlaySFX(audioManager.EnemyScreamSFX);
        Knockback(playertransform);
        Flash();
        Timestop();
        
        
        hp--;
    }
    public void Knockback(Transform playertransform)
    {
        ////Vector2 direction = (transform.position - playertransform.position).normalized;
        ////EnemyRb.linearVelocity = direction * Knockbackforce;
        //Debug.Log("ENEMY KNOCKED BACK");

        //Vector2 dir = ((Vector2)transform.position - (Vector2)playertransform.position).normalized;
        //if (dir == Vector2.zero) dir = Vector2.right;

        //// Reset velocity first so knockback isn�ft stacked
        //EnemyRb.linearVelocity = Vector2.zero;

        //// Apply knockback impulse
        //float knockbackImpulse = 5f; // tune this in Inspector
        //EnemyRb.AddForce(dir * knockbackImpulse, ForceMode2D.Impulse);
        Debug.Log("ENEMY KNOCKED BACK");

        Vector2 dir = ((Vector2)transform.position - (Vector2)playertransform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        // Clear existing velocity to avoid stacking
        EnemyRb.linearVelocity = Vector2.zero;

        // Instant shove
        EnemyRb.AddForce(dir * Knockbackforce, ForceMode2D.Impulse);

        // Put enemy in a brief "don't home back yet" window
        knockbackUntilTime = Time.time + knockbackLockout;
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
