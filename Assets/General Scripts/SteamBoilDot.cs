using UnityEngine;
using System.Collections.Generic;

public class SteamBoilDoT : MonoBehaviour
{
    [Header("Area")]
    public float lifeTime = 2.5f;
    public float radiusOverride = -1f; // -1 = use collider radius

    [Header("DoT")]
    public float tickInterval = 0.5f;
    public float damagePerTick = 3f;
    public Animator DotAnime;

    [Header("Slow While Inside")]
    public float slowMultiplier = 0.6f;  // 0.6 = 40% slow
    public float slowRefresh = 0.6f;     // refresh slow buff this often while inside

    private Transform owner; // player (for DamagePacket source)
    private CircleCollider2D circle;
    private float nextGlobalTick;
    private readonly Dictionary<Health, float> nextTickPerTarget = new();
    private readonly Dictionary<EnemyStatusController, float> slowRefreshPerTarget = new();



    public void Init(Transform owner) => this.owner = owner;

    void Awake()
    {
        // Force whatever collider you used to be a trigger
        var anyCol = GetComponent<Collider2D>();
        if (!anyCol)
        {
            Debug.LogError("SteamBoilDoT requires a Collider2D on the prefab.");
            return;
        }
        anyCol.isTrigger = true;

        // (Optional) still cache Circle if you want radius reads for gizmos/override
        circle = GetComponent<CircleCollider2D>();

        nextGlobalTick = Time.time + tickInterval;
    }

    void Start()
    {
        DotAnime.SetTrigger("SteamDot");
        
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Periodic work done in OnTriggerStay2D per target to avoid 2D overlap allocations
    }

    //void OnTriggerStay2D(Collider2D other)
    //{
    //    Debug.Log("Steam enter: " + other.name);
    //    // Damage
    //    if (other.TryGetComponent<Health>(out var hp))
    //    {
    //        float now = Time.time;
    //        if (!nextTickPerTarget.TryGetValue(hp, out var next)) next = 0f;
    //        if (now >= next)
    //        {
    //            Vector2 dir = Vector2.zero; // floor DoT shouldn't shove, but DamagePacket needs a dir
    //            float knockback = 0f;

    //            var pkt = new DamagePacket(damagePerTick, Element.Neutral, owner ? owner : transform, dir, knockback);
    //            hp.ApplyDamage(pkt, out _); // direct health call (your Health already handles multipliers)

    //            nextTickPerTarget[hp] = now + tickInterval;
    //        }
    //    }

    //    // Slow (requires EnemyStatusController on the enemy)
    //    if (other.TryGetComponent<EnemyStatusController>(out var status))
    //    {
    //        float now = Time.time;
    //        if (!slowRefreshPerTarget.TryGetValue(status, out var nextSlow)) nextSlow = 0f;
    //        if (now >= nextSlow)
    //        {
    //            status.ApplySlow(slowMultiplier, tickInterval + 0.1f); // keep it active while standing inside
    //            slowRefreshPerTarget[status] = now + slowRefresh;
    //        }
    //    }
    //}
    void OnTriggerStay2D(Collider2D other)
    {
        // (Optional) comment this out once verified; it will spam every frame:
        // Debug.Log("Steam stay: " + other.name);

        // --- DoT damage (via Enemy.Damaged so reactions fire) ---
        if (other.TryGetComponent<Health>(out var hp))
        {
            float now = Time.time;
            if (!nextTickPerTarget.TryGetValue(hp, out var next)) next = 0f;

            if (now >= next)
            {
                // Tiny outward push from floor center -> enemy, just for feedback
                Vector2 dir = (Vector2)other.transform.position - (Vector2)transform.position;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
                dir.Normalize();

                var pkt = new DamagePacket(
                    damagePerTick,
                    Element.Neutral,                       // Steam is boiling water
                    owner ? owner : transform,           // player as source if available
                    dir,
                    0.2f                                 // gentle nudge so hits "read"
                );

                if (other.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.Damaged(pkt);                  // ✅ triggers anim/flash/SFX/knockback
                }
                else
                {
                    hp.ApplyDamage(pkt, out _);          // fallback if no Enemy component
                }

                nextTickPerTarget[hp] = now + tickInterval;
            }
        }

        // --- Slow (refresh while inside) ---
        if (other.TryGetComponent<EnemyStatusController>(out var status))
        {
            float now = Time.time;
            if (!slowRefreshPerTarget.TryGetValue(status, out var nextSlow)) nextSlow = 0f;

            if (now >= nextSlow)
            {
                status.ApplySlow(slowMultiplier, tickInterval + 0.1f);
                slowRefreshPerTarget[status] = now + slowRefresh;
            }
        }
    }

}
