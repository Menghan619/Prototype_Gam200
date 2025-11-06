using UnityEngine;
using System.Collections;
using UnityEngine.UIElements.Experimental;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class FireDemonAI : MonoBehaviour
{
    public Animator animator; // Assign your enemy's Animator in the Inspector

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    [SerializeField] private float moveAnimSpeedThreshold = 0.05f; // tune

    public enum Brain { Chase, Windup, Firing, Recover, Hover }
    private Brain brain = Brain.Chase;

    [Header("Hover (cooldown behavior)")]
    public float hoverMinDist = 3.6f;      // try just under preferRange
    public float hoverMaxDist = 5.6f;      // try just over preferRange
    public float hoverSpeed = 2.8f;      // slightly lower than chase speed feels good
    [Range(0f, 1f)] public float tangentialBias = 0.8f; // 0=radial only, 1=mostly orbit
    public float hoverJitterInterval = 0.5f;            // how often to change course
    public float hoverJitterAngleDeg = 35f;             // small random turn per update

    private float nextHoverJitterTime;
    private Vector2 hoverDir = Vector2.zero;

    [Header("Refs")]
    public Transform player;            // assign in Inspector, or auto-find by tag
    public Rigidbody2D rb;              // auto-filled in Awake if null

    [Header("Movement")]
    public float moveSpeed = 3.2f;      // "medium"
    public float preferRange = 4.5f;    // sweet spot to hover at
    public float maxRange = 7.0f;       // max distance to consider shooting
    public float backoffFactor = 0.7f;  // if closer than preferRange * this, back off a bit

    [Header("Attack Timing")]
    public float windup = 0.5f;         // visible charge before firing
    public float endlag = 0.20f;        // brief delay after firing
    public float cooldown = 1.00f;      // time until next shot available
    private float nextReadyTime;

    [Header("Projectile")]
    public GameObject projectilePrefab; // your Fireball prefab
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    public float damage = 10f;
    public float knockbackForce = 3.0f;
    public LayerMask losMask;           // walls/obstacles

    private Coroutine attackCo;

    public Facing2D facing;  // drag in from prefab or auto-find

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!facing) facing = GetComponent<Facing2D>();               // same object
        if (!facing) facing = GetComponentInChildren<Facing2D>();     // or child
        // Rigidbody safety defaults
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (!player) return;

        if (brain == Brain.Chase && Time.time >= nextReadyTime)
        {
            Vector2 to = player.position - transform.position;
            float dist = to.magnitude;

            if (dist <= maxRange && HasLoS(to))
            {
                // start an attack cycle
                attackCo = StartCoroutine(AttackCycle());
                return;
            }
        }
        // When hovering and cooldown expired: try to attack; if not eligible, go back to Chase
        if (brain == Brain.Hover && Time.time >= nextReadyTime)
        {
            Vector2 to = player.position - transform.position;
            float dist = to.magnitude;
            if (dist <= maxRange && HasLoS(to))
            {
                attackCo = StartCoroutine(AttackCycle());
            }
            else
            {
                brain = Brain.Chase;
            }
        }
        if (brain == Brain.Chase && Time.time < nextReadyTime)
            brain = Brain.Hover;
    }
    /*
    void FixedUpdate()
    {
        if (!player) return;

        // NEW: don't override physics knockback
        var enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.InKnockback) return;

        if (brain != Brain.Chase) return;

        if (!player || brain != Brain.Chase) return;

        Vector2 to = (player.position - transform.position);
        float dist = to.magnitude;
        //Vector2 dir = to.sqrMagnitude > 0.001f ? to.normalized : Vector2.zero; OLD
        Vector2 dir = dist > 0.0001f ? to / dist : Vector2.right;

        //// keep around preferRange (simple stutter step)
        //if (dist > preferRange * 1.1f)
        //{
        //    // move closer
        //    rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        //    if (facing) facing.SetMovementDir(dir);            // face movement direction (toward player)
        //}
        //else if (dist < preferRange * backoffFactor)
        //{
        //    // back off a bit
        //    var away = -dir;
        //    rb.MovePosition(rb.position - dir * (moveSpeed * 0.6f) * Time.fixedDeltaTime);
        //    if (facing) facing.SetMovementDir(away);           // face the away direction while retreating
        //}
        //// else: within band �� hold position 
        //======== OLD =====================

        switch (brain)
        {
            case Brain.Chase:
                {
                    if (dist > preferRange * 1.1f)
                    {
                        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
                        facing?.SetMovementDir(dir);
                    }
                    else if (dist < preferRange * backoffFactor)
                    {
                        Vector2 away = -dir;
                        rb.MovePosition(rb.position + away * (moveSpeed * 0.6f) * Time.fixedDeltaTime);
                        facing?.SetMovementDir(away);
                    }
                    // else hold
                    break;
                }

            case Brain.Hover:
                {
                    // Recompute a hover direction occasionally
                    if (Time.time >= nextHoverJitterTime || hoverDir == Vector2.zero)
                    {
                        nextHoverJitterTime = Time.time + hoverJitterInterval;

                        // Base tangential around the player (left or right randomly)
                        Vector2 tangent = new Vector2(-dir.y, dir.x);
                        if (Random.value < 0.5f) tangent = -tangent;

                        // Radial correction to stay in the band
                        Vector2 radial = Vector2.zero;
                        if (dist < hoverMinDist) radial = -dir;       // push outward
                        else if (dist > hoverMaxDist) radial = dir;   // pull inward

                        // Mix tangent with radial correction
                        Vector2 desired = tangent * tangentialBias + radial * (1f - tangentialBias);

                        // Add small random turn
                        float ang = Random.Range(-hoverJitterAngleDeg, hoverJitterAngleDeg) * Mathf.Deg2Rad;
                        float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
                        desired = new Vector2(desired.x * ca - desired.y * sa, desired.x * sa + desired.y * ca);

                        if (desired.sqrMagnitude < 0.0001f) desired = tangent; // fallback
                        hoverDir = desired.normalized;
                    }

                    // Move along hoverDir at hoverSpeed
                    rb.MovePosition(rb.position + hoverDir * hoverSpeed * Time.fixedDeltaTime);
                    facing?.SetMovementDir(hoverDir);
                    break;
                }

                // Windup/Firing/Recover handled by coroutine; no locomotion here
        }
    }*/
    //
    void FixedUpdate()
    {

        if (!player) return;

        // Don’t fight physics knockback
        var enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.InKnockback)
        {

            // If you want the move anim during knockback, use rb.linearVelocity magnitude:
            if (animator) animator.SetBool(IsMovingHash, rb.linearVelocity.magnitude > moveAnimSpeedThreshold);
            return;

        }

        Vector2 to = (Vector2)player.position - (Vector2)transform.position;
        float dist = to.magnitude;
        Vector2 dir = dist > 0.0001f ? to / dist : Vector2.right;

        bool movedThisTick = false;
        switch (brain)
        {
            //case Brain.Chase:
            //    {
            //        float spdMul = 1f;
            //        var status = GetComponent<EnemyStatusController>();
            //        if (status) spdMul = status.CurrentSpeedMultiplier;
            //        if (dist > preferRange * 1.1f)
            //        {
            //            //rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            //            // when moving closer:
            //            rb.MovePosition(rb.position + dir * (moveSpeed * spdMul) * Time.fixedDeltaTime);

            //            facing?.SetMovementDir(dir);
            //        }
            //        else if (dist < preferRange * backoffFactor)
            //        {
            //            Vector2 away = -dir;
            //            //rb.MovePosition(rb.position + away * (moveSpeed * 0.6f) * Time.fixedDeltaTime);
            //            // when backing off:
            //            rb.MovePosition(rb.position + away * (moveSpeed * 0.6f * spdMul) * Time.fixedDeltaTime);
            //            movedThisTick = step.sqrMagnitude > (moveAnimSpeedThreshold * moveAnimSpeedThreshold);

            //            facing?.SetMovementDir(away);
            //        }
            //        // else: hold
            //        break;
            //    }
            case Brain.Chase:
                {
                    float spdMul = 1f;
                    var status = GetComponent<EnemyStatusController>();
                    if (status) spdMul = status.CurrentSpeedMultiplier;

                    if (dist > preferRange * 1.1f)
                    {
                        Vector2 step = dir * (moveSpeed * spdMul) * Time.fixedDeltaTime;
                        rb.MovePosition(rb.position + step);
                        facing?.SetMovementDir(dir);
                        movedThisTick = step.sqrMagnitude > (moveAnimSpeedThreshold * moveAnimSpeedThreshold);
                    }
                    else if (dist < preferRange * backoffFactor)
                    {
                        Vector2 away = -dir;
                        Vector2 step = away * (moveSpeed * 0.6f * spdMul) * Time.fixedDeltaTime;
                        rb.MovePosition(rb.position + step);
                        facing?.SetMovementDir(away);
                        movedThisTick = step.sqrMagnitude > (moveAnimSpeedThreshold * moveAnimSpeedThreshold);
                    }
                    break;
                }

            //case Brain.Hover:
            //    {
            //        // Recompute hoverDir every jitter interval
            //        if (Time.time >= nextHoverJitterTime || hoverDir == Vector2.zero)
            //        {
            //            nextHoverJitterTime = Time.time + hoverJitterInterval;

            //            // Tangent around the player (random side)
            //            Vector2 tangent = new Vector2(-dir.y, dir.x);
            //            if (Random.value < 0.5f) tangent = -tangent;

            //            // Radial correction to stay inside band
            //            Vector2 radial = Vector2.zero;
            //            if (dist < hoverMinDist) radial = -dir;       // push outward
            //            else if (dist > hoverMaxDist) radial = dir;   // pull inward

            //            Vector2 desired = tangent * tangentialBias + radial * (1f - tangentialBias);

            //            // Small random turn
            //            float ang = Random.Range(-hoverJitterAngleDeg, hoverJitterAngleDeg) * Mathf.Deg2Rad;
            //            float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
            //            desired = new Vector2(desired.x * ca - desired.y * sa, desired.x * sa + desired.y * ca);

            //            if (desired.sqrMagnitude < 0.0001f) desired = tangent;
            //            hoverDir = desired.normalized;
            //        }
            //        float spdMul = 1f;
            //        var status = GetComponent<EnemyStatusController>();
            //        if (status) spdMul = status.CurrentSpeedMultiplier;
            //        // Move along hoverDir
            //        //rb.MovePosition(rb.position + hoverDir * hoverSpeed * Time.fixedDeltaTime);
            //        rb.MovePosition(rb.position + hoverDir * (hoverSpeed * spdMul) * Time.fixedDeltaTime);

            //        facing?.SetMovementDir(hoverDir);
            //        break;
            //    }
            case Brain.Hover:
                {
                    if (Time.time >= nextHoverJitterTime || hoverDir == Vector2.zero)
                    {
                        nextHoverJitterTime = Time.time + hoverJitterInterval;

                        Vector2 tangent = new Vector2(-dir.y, dir.x);
                        if (Random.value < 0.5f) tangent = -tangent;

                        Vector2 radial = Vector2.zero;
                        if (dist < hoverMinDist) radial = -dir;       // push outward
                        else if (dist > hoverMaxDist) radial = dir;   // pull inward

                        Vector2 desired = tangent * tangentialBias + radial * (1f - tangentialBias);

                        float ang = Random.Range(-hoverJitterAngleDeg, hoverJitterAngleDeg) * Mathf.Deg2Rad;
                        float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
                        desired = new Vector2(desired.x * ca - desired.y * sa, desired.x * sa + desired.y * ca);

                        if (desired.sqrMagnitude < 0.0001f) desired = tangent;
                        hoverDir = desired.normalized;
                    }
                    float spdMul = 1f;
                    var status = GetComponent<EnemyStatusController>();
                    if (status) spdMul = status.CurrentSpeedMultiplier;

                    Vector2 step = hoverDir * (hoverSpeed * spdMul) * Time.fixedDeltaTime;
                    rb.MovePosition(rb.position + step);
                    facing?.SetMovementDir(hoverDir);
                    movedThisTick = step.sqrMagnitude > (moveAnimSpeedThreshold * moveAnimSpeedThreshold);
                    break;

                    // Windup/Firing/Recover are handled by the Attack coroutine; no locomotion here.
                }


        }
    }


    bool HasLoS(Vector2 toPlayer)
    {
        var hit = Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, losMask);
        return hit.collider == null;
    }

    IEnumerator AttackCycle()
    {
        brain = Brain.Windup;
        if (animator) animator.SetBool(IsMovingHash, false);

        // face lock + charge
        Vector2 shotDir = player ? (player.position - transform.position).normalized : Vector2.right;
        // Lock facing for the whole attack window so it doesn't flip mid-shot
        if (facing) facing.BeginAttackFacing(shotDir, windup + endlag + 0.05f);

        // Trigger charge animation
        if (animator) animator.SetTrigger("FireCharge");
        // TODO: play charge VFX/SFX here (anim trigger if you have one)
        yield return new WaitForSeconds(windup);

        if (!player) { brain = Brain.Chase; yield break; }

        // FIRE
        brain = Brain.Firing;
        if (animator) animator.SetTrigger("FireEnemyAttack"); // optional shoot anim trigger
        FireOneShot();

        brain = Brain.Recover;
        yield return new WaitForSeconds(endlag);

        nextReadyTime = Time.time + cooldown;
        //brain = Brain.Chase;
        brain = Brain.Hover;
        if (facing) facing.EndAttackFacing(); // explicit unlock (also auto-unlocks by time)

        attackCo = null;
    }

    void FireOneShot()
    {
        if (!projectilePrefab || !player) return;

        Vector2 dir = (player.position - transform.position).normalized;

        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Rigidbody2D motion
        if (go.TryGetComponent<Rigidbody2D>(out var prb))
        {
            prb.gravityScale = 0f;
            prb.linearVelocity = dir * projectileSpeed;
            prb.interpolation = RigidbodyInterpolation2D.Interpolate;
            prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Configure projectile
        if (go.TryGetComponent<SimpleProjectile>(out var proj))
        {
            proj.lifetime = projectileLifetime;
            // Optional: set proj.hitLayers in the Inspector on the prefab to Enemy layer
            var pkt = new DamagePacket(damage, Element.Fire, transform, dir, knockbackForce);
            proj.Setup(pkt, transform);
        }
    }

    void OnDisable()
    {
        if (attackCo != null) StopCoroutine(attackCo);
    }
}
