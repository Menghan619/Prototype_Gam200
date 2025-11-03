using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SimpleProjectile : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 5f;

    [Header("Collision")]
    public LayerMask hitLayers;     // which layers count as targets (e.g., Enemy)
    public bool destroyOnAnyNonTrigger = true;

    private DamagePacket packet;
    private Transform owner;        // who fired this (so we don't hit the owner)
    private bool armed;

    void OnEnable()
    {
        // auto-despawn
        Invoke(nameof(Despawn), Mathf.Max(0.01f, lifetime));
    }

    /// <summary>Set up damage + owner before firing.</summary>
    public void Setup(DamagePacket pkt, Transform owner)
    {
        packet = pkt;
        this.owner = owner;
        armed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;

        // ignore owner/self hits
        if (owner && (other.transform == owner || other.transform.IsChildOf(owner))) return;

        // layer filter (optional: if unset, accept all)
        if (hitLayers.value != 0 && (hitLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            if (destroyOnAnyNonTrigger && !other.isTrigger) Despawn();
            return;
        }

        // damage Player first if present
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeHit(transform);   // uses projectile position to compute knockback direction
            Despawn();
            return;
        }

        // prefer Enemy script so you keep your knockback/hitstop feedback
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Damaged(packet);
            Despawn();
            return;
        }

        // If you ever want to damage non-Enemy things that implement IDamageable:
        // if (other.TryGetComponent<IDamageable>(out var dmg))
        // {
        //     dmg.ApplyDamage(packet);
        //     Despawn();
        //     return;
        // }

        if (destroyOnAnyNonTrigger && !other.isTrigger) Despawn();
    }

    private void Despawn()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}
