using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterDemonAI : MonoBehaviour
{
    public enum Brain { Chase, Windup, Pulse, Recover, Hover }
    private Brain brain = Brain.Chase;

    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;          // optional: charge/pulse triggers
    public Facing2D facing;            // faces movement; lock during attack

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float stopBand = 0.15f;

    [Header("AoE Attack")]
    public float aoeRadius = 2.2f;     // must be inside this to start Windup
    public float windup = 0.25f;    // pre-pulse charge
    public float recover = 0.25f;    // brief opening after pulse
    public float cooldown = 1.20f;    // overall cooldown between attacks
    public string chargeTrigger = "WaterCharge"; // optional
    public string pulseTrigger = "WaterPulse";  // optional

    [Header("Hover (cooldown behaviour)")]
    public float hoverMinDist = 2.8f;  // band around player
    public float hoverMaxDist = 4.2f;
    public float hoverSpeed = 2.2f;  // a touch slower than chase feels ÅgheavierÅh
    [Range(0f, 1f)] public float tangentialBias = 0.8f; // 0=radial, 1=orbit
    public float hoverJitterInterval = 0.55f;          // how often to redirect
    public float hoverJitterAngleDeg = 30f;            // small random turn

    private float nextReadyTime;
    private Coroutine attackCo;

    // hover state helpers
    private float nextHoverJitterTime;
    private Vector2 hoverDir = Vector2.zero;

    [Header("Audio")]
    AudioManager audioManager;
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!facing) facing = GetComponent<Facing2D>() ?? GetComponentInChildren<Facing2D>();

        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;


        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();
    }

    void Update()
    {
        if (!player) return;

        // Start an attack when ready AND in range (donÅft cancel once started)
        if (brain == Brain.Chase && Time.time >= nextReadyTime && IsInAoeRange())
        {
            attackCo = StartCoroutine(AttackCycle());
            // start cooldown at attack start so hovering doesnÅft spam
            nextReadyTime = Time.time + cooldown;
            return;
        }

        // In Hover: when cooldown ends, either attack (if in range) or go back to Chase
        if (brain == Brain.Hover && Time.time >= nextReadyTime)
        {
            if (IsInAoeRange())
            {
                attackCo = StartCoroutine(AttackCycle());
                nextReadyTime = Time.time + cooldown;
            }
            else
            {
                brain = Brain.Chase;
            }
        }

        // Optional: if weÅfre in Chase but still cooling down (e.g., just fired), switch to Hover
        if (brain == Brain.Chase && Time.time < nextReadyTime)
        {
            brain = Brain.Hover;
        }
    }

    void FixedUpdate()
    {
        if (!player) return;

        // DonÅft fight physics knockback from hits
        var enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.InKnockback) return;

        Vector2 to = (Vector2)player.position - (Vector2)transform.position;
        float dist = to.magnitude;
        Vector2 dir = dist > 0.0001f ? to / dist : Vector2.right;

        switch (brain)
        {
            case Brain.Chase:
                {
                    if (dist > stopBand)
                    {
                        //rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
                        float spdMul = 1f;
                        var status = GetComponent<EnemyStatusController>();
                        if (status) spdMul = status.CurrentSpeedMultiplier;

                        // when moving closer:
                        rb.MovePosition(rb.position + dir * (moveSpeed * spdMul) * Time.fixedDeltaTime);

                        // when backing off:
                        //rb.MovePosition(rb.position - dir * (moveSpeed * 0.6f * spdMul) * Time.fixedDeltaTime);

                        facing?.SetMovementDir(dir);
                    }
                    break;
                }

            case Brain.Hover:
                {
                    // Recompute a hover vector periodically
                    if (Time.time >= nextHoverJitterTime || hoverDir == Vector2.zero)
                    {
                        nextHoverJitterTime = Time.time + hoverJitterInterval;

                        // Tangent left/right around player
                        Vector2 tangent = new Vector2(-dir.y, dir.x);
                        if (Random.value < 0.5f) tangent = -tangent;

                        // Radial correction to keep inside band
                        Vector2 radial = Vector2.zero;
                        if (dist < hoverMinDist) radial = -dir;       // push outward
                        else if (dist > hoverMaxDist) radial = dir;   // pull inward

                        Vector2 desired = tangent * tangentialBias + radial * (1f - tangentialBias);

                        // Small random turn
                        float ang = Random.Range(-hoverJitterAngleDeg, hoverJitterAngleDeg) * Mathf.Deg2Rad;
                        float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
                        desired = new Vector2(desired.x * ca - desired.y * sa, desired.x * sa + desired.y * ca);

                        if (desired.sqrMagnitude < 0.0001f) desired = tangent;
                        hoverDir = desired.normalized;
                    }
                    float spdMul = 1f;
                    var status = GetComponent<EnemyStatusController>();
                    if (status) spdMul = status.CurrentSpeedMultiplier;

                    //rb.MovePosition(rb.position + hoverDir * hoverSpeed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + hoverDir * (hoverSpeed * spdMul) * Time.fixedDeltaTime);

                    facing?.SetMovementDir(hoverDir);
                    break;
                }

                // Windup/Pulse/Recover locomotion is handled by coroutine timing (we stand still)
        }
    }

    bool IsInAoeRange()
    {
        return player && Vector2.Distance(player.position, transform.position) <= aoeRadius;
    }

    IEnumerator AttackCycle()
    {
        // WINDUP
        brain = Brain.Windup;

        Vector2 faceDir = player ? ((Vector2)player.position - (Vector2)transform.position).normalized : Vector2.right;
        facing?.BeginAttackFacing(faceDir, windup + recover + 0.05f);
        audioManager.PlaySFX(audioManager.WaterDemonChargeAttack);
        if (animator && !string.IsNullOrEmpty(chargeTrigger)) animator.SetTrigger(chargeTrigger);
        yield return new WaitForSeconds(windup);

        // PULSE (one-shot)
        brain = Brain.Pulse;
        if (animator && !string.IsNullOrEmpty(pulseTrigger)) animator.SetTrigger(pulseTrigger);
        

        var ph = player ? player.GetComponent<PlayerHealth>() : null;
        if (ph && Vector2.Distance(player.position, transform.position) <= aoeRadius)
        {
            ph.TakeHit(transform);
        }

        // RECOVER
        brain = Brain.Recover;
        yield return new WaitForSeconds(recover);

        // After an attack cycle, go into Hover until cooldown elapses
        brain = Brain.Hover;
        facing?.EndAttackFacing();
        attackCo = null;
    }

    void OnDisable()
    {
        if (attackCo != null) StopCoroutine(attackCo);
        attackCo = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, hoverMinDist);
        Gizmos.DrawWireSphere(transform.position, hoverMaxDist);
    }
}
