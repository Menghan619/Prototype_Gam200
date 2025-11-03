using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class WaterDemonDeathExplode : MonoBehaviour
{
    [Header("Refs")]
    public Health health;
    public Rigidbody2D rb;
    public Animator animator;        // optional
    public Facing2D facing;          // optional but nice for locking toward player
    public Transform player;         // auto-find by Player tag if left empty
    public WaterDemonAI ai;          // disable AI during death sequence

    [Header("Death Explosion")]
    [Tooltip("Short charge before the suicide explosion.")]
    public float chargeTime = 0.5f;
    [Tooltip("Explosion radius around this enemy.")]
    public float explodeRadius = 2.6f;
    [Tooltip("Animator trigger names (optional).")]
    public string breakTrigger = "ShieldBreak";
    public string chargeTrigger = "SuicideCharge";

    [Header("Explosion Hit Filter")]
    public LayerMask playerLayers;   // set this to your Player layer(s) in Inspector
    private readonly Collider2D[] _hits = new Collider2D[8]; // small buffer

    void Awake()
    {
        if (!health) health = GetComponent<Health>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!facing) facing = GetComponent<Facing2D>() ?? GetComponentInChildren<Facing2D>();
        if (!ai) ai = GetComponent<WaterDemonAI>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeathStart;
    }

    void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleDeathStart;
    }

    void HandleDeathStart()
    {
        // Stop normal AI/motion
        if (ai) ai.enabled = false;

        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // freeze in place for the charge
        }

        // Face player during the charge (optional lock)
        if (facing && player)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            facing.BeginAttackFacing(dir, chargeTime + 0.1f);
        }

        // Play shield-break / charge visuals (optional)
        if (animator && !string.IsNullOrEmpty(breakTrigger)) animator.SetTrigger(breakTrigger);
        if (animator && !string.IsNullOrEmpty(chargeTrigger)) animator.SetTrigger("WaterHit");

        StartCoroutine(CoExplodeAfterCharge());
    }

    IEnumerator CoExplodeAfterCharge()
    {
        // Use realtime so any hitstop elsewhere doesn't stall death sequence
        float t = 0f;
        while (t < chargeTime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        //// One-shot AoE: damage player if within radius
        //var ph = player ? player.GetComponent<PlayerHealth>() : null;
        //if (ph && Vector2.Distance(player.position, transform.position) <= explodeRadius)
        //{
        //    ph.TakeHit(transform); // your 1-heart + knockback + i-frames
        //}

        //// Destroy self cleanly
        //if (health != null) health.DestroyNow();
        //else Destroy(gameObject);

        // AoE: fetch all colliders on playerLayers within explodeRadius
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position,
            explodeRadius,
            playerLayers
        );

        // Apply damage to any PlayerHealth found among overlaps
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col) continue;

            var ph = col.GetComponent<PlayerHealth>()
                  ?? col.GetComponentInParent<PlayerHealth>()
                  ?? col.GetComponentInChildren<PlayerHealth>();

            if (ph != null)
            {
                ph.TakeHit(transform);   // 1 heart + knockback + i-frames
                                         // break; // uncomment if you only want to hit the player once
            }
        }
        // Destroy self cleanly
        if (health != null) health.DestroyNow();
        else Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
