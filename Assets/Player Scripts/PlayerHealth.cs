using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UIElements;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Material to switch to during the flash.")]
    [SerializeField] private Material flashMaterial;

    [Tooltip("Duration of the flash.")]
    [SerializeField] private float duration;

    // The SpriteRenderer that should flash.
    private SpriteRenderer spriteRenderer;

    AudioManager audioManager;

    // The material that was in use, when the script started.
    private Material originalMaterial;

    // The currently running coroutine.
    private Coroutine flashRoutine;

    [Tooltip("HP sprtie")]
    [SerializeField] private Sprite LossHP;
    [SerializeField] private Sprite OGSPRITE;
    [SerializeField] private UnityEngine.UI.Image HP1;
    [SerializeField] private UnityEngine.UI.Image HP2;
    [SerializeField] private UnityEngine.UI.Image HP3;
    [SerializeField] private GameObject HP1s;
    [SerializeField] private GameObject HP2s;
    [SerializeField] private GameObject HP3s;
    [SerializeField] private GameObject ManaBar;
    //


    [Header("Hearts")]
    public int maxHearts = 3;
    public int currentHearts = 3;

    [Header("Hurt / I-Frames")]
    public float invulnDuration = 0.8f;     // total invulnerability time
    public float staggerDuration = 0.15f;   // short input lock at start of i-frames
    public float blinkInterval = 0.08f;     // sprite blink cadence

    [Header("Knockback")]
    public float knockbackForce = 3.5f;     // constant force for all hits

    [Header("Refs")]
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator animator;
    [Tooltip("Trigger name in the Animator to play on hurt.")]
    public string hurtTrigger = "Hurt";

    [Tooltip("Your player movement component (for input lock).")]
    public MonoBehaviour movementScript;      // assign Player Movement script here
    [Tooltip("If the movement script has a bool property to lock input, put its name here (e.g., IsInputLocked). If empty, we toggle the component.")]
    public string movementLockProperty = "IsInputLocked";

    // Runtime state
    public bool IsInvulnerable { get; private set; }

    public event Action OnDamaged;  // score system can listen
    public event Action OnDeath;    // TODO hookup later


    // Add near your other fields:
    private bool isDead = false;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);
        OGSPRITE = HP1.sprite;
        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the material that the SpriteRenderer uses, 
        // so we can switch back to it after the flash ended.
        originalMaterial = spriteRenderer.material;
    }
    private void Update()
    {
        if (currentHearts == 2)
        {
            HP1.sprite = OGSPRITE;
            HP2.sprite = OGSPRITE;
            HP3.sprite = LossHP;
        }
        else if (currentHearts == 1)
        {
            HP1.sprite = OGSPRITE;
            HP2.sprite = LossHP;
            HP3.sprite = LossHP;
        }
        else if (currentHearts == 0)
        {
            HP1.sprite = LossHP;
            HP2.sprite = LossHP;
            HP3.sprite = LossHP;

        }
    }
    // --- Public API (always 1 heart, same knockback) ---
    public void TakeHit(Transform source) => TakeHit((Vector2)source.position);

    //public void TakeHit(Vector2 hitFromPosition)
    //{
    //    if (IsInvulnerable || currentHearts <= 0) return;

    //    // Animator hurt
    //    if (animator && !string.IsNullOrEmpty(hurtTrigger))
    //        animator.SetTrigger(hurtTrigger);

    //    // Knockback (away from hit origin)
    //    if (rb)
    //    {
    //        Vector2 dir = ((Vector2)transform.position - hitFromPosition);
    //        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
    //        dir.Normalize();

    //        rb.linearVelocity = Vector2.zero; // crisp feel
    //        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
    //    }

    //    // Hearts
    //    currentHearts = Mathf.Max(0, currentHearts - 1);

    //    // Notify
    //    OnDamaged?.Invoke();
    //    Flash();
    //    // I-frames + stagger + blink
    //    StartCoroutine(HurtFlow());

    //    // Death stub
    //    if (currentHearts <= 0)
    //    {
    //        // ======= TODO: Player death handling (UI, anim, respawn, etc.) =======
    //        OnDeath?.Invoke();
    //    }
    //}
    public void TakeHit(Vector2 hitFromPosition)
    {
        if (IsInvulnerable || currentHearts <= 0 || isDead)
        {

            HP1.sprite = LossHP;
            HP2.sprite = LossHP;
            HP3.sprite = LossHP;
            


            return;


        }

        // Hurt anim only if we’re not going to die from this hit
        // (we’ll trigger the real death anim in Die())
        // We'll decide after decrement.

        // Knockback (away from hit origin)
        if (rb)
        {
            Vector2 dir = ((Vector2)transform.position - hitFromPosition);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            dir.Normalize();

            rb.linearVelocity = Vector2.zero; // crisp feel
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        // Hearts
        currentHearts = Mathf.Max(0, currentHearts - 1);
        
        
            OnDamaged?.Invoke();

            if (currentHearts <= 0)
            {
                Die();               // <<< go straight to death flow
                return;
            }

            // Only do hurt feedback if we’re still alive:
            
        if (animator && !string.IsNullOrEmpty(hurtTrigger))
                animator.SetTrigger(hurtTrigger);
        

            
        Flash();
        audioManager.PlaySFX(audioManager.PlayerDamage);

        StartCoroutine(HurtFlow());
        
    }
    

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop any running hurt flashes/blinks and ensure visible
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        if (sprite) sprite.enabled = true;

        // Disable control scripts cleanly
        SetMovementLocked(true);
        var abil = GetComponent<Abilities>();
        if (abil) abil.enabled = false;

        // Make sure we don’t take further hits during the sequence
        IsInvulnerable = true;

        // Trigger your actual DEATH animation
        // (Set this trigger name to your real one in the inspector if needed)

        if (animator) animator.SetTrigger("PlayerDeath");
        audioManager.PlaySFX(audioManager.PlayerDeath);
        HP1.enabled = false;
        HP2.enabled = false;
        HP3.enabled = false;
        HP1s.SetActive(false);
        HP2s.SetActive(false);
        HP3s.SetActive(false);
        ManaBar.SetActive(false);

        // Notify the outside world (GameFlowManager will handle timing + fade)
        OnDeath?.Invoke();
    }

    private IEnumerator HurtFlow()
    {
        IsInvulnerable = true;

        // Stagger input (realtime so hitstop elsewhere won't freeze it)
        SetMovementLocked(true);
        yield return new WaitForSecondsRealtime(staggerDuration);
        SetMovementLocked(false);

        // Blink until invuln ends
        float t = 0f;
        bool vis = true;
        while (t < invulnDuration)
        {
            if (sprite)
            {
                vis = !vis;
                sprite.enabled = vis;
            }
            float step = Mathf.Min(blinkInterval, invulnDuration - t);
            t += step;
            yield return new WaitForSecondsRealtime(step);
        }
        if (sprite) sprite.enabled = true;

        IsInvulnerable = false;
    }

    private void SetMovementLocked(bool locked)
    {
        if (!movementScript) return;

        // Try to set a bool property (e.g., IsInputLocked) if it exists
        var t = movementScript.GetType();
        var p = t.GetProperty(movementLockProperty);
        if (!string.IsNullOrEmpty(movementLockProperty) && p != null && p.PropertyType == typeof(bool) && p.CanWrite)
        {
            p.SetValue(movementScript, locked);
        }
        else
        {
            // Fallback: enable/disable the movement component
            movementScript.enabled = !locked;
        }

        // If movement has a Stop() method, call it when locking to halt motion
        if (locked)
        {
            var stop = t.GetMethod("Stop", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (stop != null) stop.Invoke(movementScript, null);
        }
    }

    // Optional helpers for Shrine upgrades later
    public void HealHearts(int n) => currentHearts = Mathf.Clamp(currentHearts + Mathf.Abs(n), 0, maxHearts);
    public void SetMaxHearts(int newMax, bool fill = true)
    {
        maxHearts = Mathf.Max(1, newMax);
        if (fill) currentHearts = maxHearts;
        else currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);
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

    // --- Dash / utility i-frames (independent of HurtFlow) ---
    public void GrantTemporaryInvulnerability(float durationSeconds, bool blink = false)
    {
        StartCoroutine(CoTempInvuln(durationSeconds, blink));
    }

    private IEnumerator CoTempInvuln(float durationSeconds, bool blink)
    {
        IsInvulnerable = true;

        if (blink && sprite)
        {
            float t = 0f;
            bool vis = true;
            while (t < durationSeconds)
            {
                vis = !vis;
                sprite.enabled = vis;
                float step = Mathf.Min(blinkInterval, durationSeconds - t);
                t += step;
                yield return new WaitForSecondsRealtime(step);
            }
            sprite.enabled = true;
        }
        else
        {
            yield return new WaitForSecondsRealtime(durationSeconds);
        }

        IsInvulnerable = false;
    }
}
