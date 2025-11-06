//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(Animator))]
//public class EnemyDeathHandler : MonoBehaviour
//{
//    [Header("Optional: override delay if no animation clip length is found")]
//    [SerializeField] private float fallbackDeathDelay = 0.8f;

//    [Tooltip("Destroy the entire enemy GameObject after animation plays")]
//    [SerializeField] private bool destroyAfterDeath = true;

//    [Tooltip("Optional: animator trigger for death animation")]
//    [SerializeField] private string deathTriggerName = "Death";

//    //[Tooltip("Optional: choose enemy type")]
//    //public Element defenseElement = Element.Neutral; // optional mirror for convenience

//    private Animator anim;
//    private Health health;
//    private bool dying = false;



//    void Awake()
//    {
//        anim = GetComponent<Animator>();
//        health = GetComponent<Health>();
//        if (health)
//        {
//            health.OnDeath += HandleDeath; // subscribe to death event
//        }
//    }

//    private void OnDestroy()
//    {
//        if (health) health.OnDeath -= HandleDeath;
//    }

//    private void HandleDeath()
//    {
//        if (dying) return;
//        dying = true;

//        // 1️⃣ Stop all AI behaviour
//        StopEnemyAI();

//        // 2️⃣ Trigger death animation
//        if (anim && !string.IsNullOrEmpty(deathTriggerName))
//        {
//            anim.SetTrigger(deathTriggerName);
//        }

//        // 3️⃣ Calculate delay (animation length or fallback)
//        float delay = fallbackDeathDelay;
//        if (anim && anim.runtimeAnimatorController)
//        {
//            foreach (var clip in anim.runtimeAnimatorController.animationClips)
//            {
//                if (clip.name.ToLower().Contains("death"))
//                {
//                    delay = clip.length;
//                    break;
//                }
//            }
//        }

//        // 4️⃣ Begin cleanup sequence
//        StartCoroutine(DeathSequence(delay));
//    }

//    private IEnumerator DeathSequence(float delay)
//    {
//        yield return new WaitForSeconds(3f);

//        if (destroyAfterDeath)
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void StopEnemyAI()
//    {
//        // Disable AI scripts (FireDemonAI, WaterDemonAI, etc.)
//        var fireAI = GetComponent<FireDemonAI>();
//        if (fireAI) fireAI.enabled = false;

//        var waterAI = GetComponent<WaterDemonAI>();
//        if (waterAI) waterAI.enabled = false;

//        // Optionally disable facing/flipping
//        var facing = GetComponent<Facing2D>();
//        if (facing) facing.enabled = false;

//        // Stop Rigidbody2D motion
//        var rb = GetComponent<Rigidbody2D>();
//        if (rb)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.simulated = false; // freezes physics without removing Rigidbody
//        }

//        // Disable colliders to prevent player attacks from hitting dead body
//        foreach (var col in GetComponents<Collider2D>())
//        {
//            col.enabled = false;
//        }
//    }
//}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class EnemyDeathHandler : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float fallbackDeathDelay = 0.8f; // if we can't find a clip length

    [Header("Cleanup")]
    [SerializeField] private bool destroyAfterDeath = true;

    [Header("Corpse Fade-Out")]
    [Tooltip("If true, fade SpriteRenderers after the death animation, then destroy.")]
    [SerializeField] private bool fadeOutOnDeath = true;
    [SerializeField] private float fadeDuration = 0.6f;      // seconds
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private Animator anim;
    private Health health;
    private bool dying = false;

    // cache of sprites to fade
    private List<SpriteRenderer> sprites = new List<SpriteRenderer>();

    void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
        if (health) health.OnDeath += HandleDeath;

        // cache all sprites (root + children)
        GetComponentsInChildren(true, sprites);
    }

    void OnDestroy()
    {
        if (health) health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (dying) return;
        dying = true;

        // 1) Stop all enemy behaviour immediately
        StopEnemyAI();

        // 2) Trigger death anim
        

        // 3) Figure out how long to wait (death clip length or fallback)
        float delay = fallbackDeathDelay;
        if (anim && anim.runtimeAnimatorController)
        {
            foreach (var clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip && clip.name.ToLower().Contains("death"))
                {
                    delay = clip.length;
                    break;
                }
            }
        }

        // 4) Sequence: wait for death anim → optional fade → destroy
        StartCoroutine(CoDeathSequence(delay));
    }

    private IEnumerator CoDeathSequence(float deathAnimTime)
    {
        // wait for the animation time (game-time, not realtime, so it respects any animator speed)
        //yield return new WaitForSeconds(deathAnimTime);

        if (fadeOutOnDeath && sprites.Count > 0 && fadeDuration > 0f)
            yield return StartCoroutine(CoFadeSprites());

        if (destroyAfterDeath)
            Destroy(gameObject);
    }

    private IEnumerator CoFadeSprites()
    {

        if (anim && !string.IsNullOrEmpty(deathTriggerName))
            anim.SetTrigger(deathTriggerName);
        // capture starting colors
        var startColors = new Color[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i]) startColors[i] = sprites[i].color;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled so fade isn't affected by hitstop/timeScale
            float k = Mathf.Clamp01(t / fadeDuration);
            float a = Mathf.Clamp01(fadeCurve.Evaluate(k)); // 1→0

            for (int i = 0; i < sprites.Count; i++)
            {
                var sr = sprites[i];
                if (!sr) continue;
                var c = startColors[i];
                c.a = a;
                sr.color = c;
            }

            yield return null;
        }

        // ensure fully invisible
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i])
            {
                var c = sprites[i].color;
                c.a = 0f;
                sprites[i].color = c;
            }
        }
    }

    private void StopEnemyAI()
    {
        // Disable specific AI scripts you use
        var fireAI = GetComponent<FireDemonAI>(); if (fireAI) fireAI.enabled = false;
        var waterAI = GetComponent<WaterDemonAI>(); if (waterAI) waterAI.enabled = false;

        // Stop Facing2D flipping
        var facing = GetComponent<Facing2D>(); if (facing) facing.enabled = false;

        // Freeze physics
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            rb.angularVelocity = 0f;
            rb.simulated = false; // clean freeze
        }

        // Disable all 2D colliders (root + children)
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }
}
