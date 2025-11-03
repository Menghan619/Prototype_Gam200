//using UnityEngine;

//public class Facing2D : MonoBehaviour
//{
//    [Header("Refs")]
//    public SpriteRenderer sprite;      // assign your enemy's SpriteRenderer (or auto-find)
//    [Header("Tuning")]
//    public bool invert = false;        // toggle if your art is mirrored
//    [Range(0f, 0.25f)] public float xDeadzone = 0.05f; // ignore tiny x changes to avoid jitter

//    private bool locked = false;       // when true, movement won't change facing
//    private float lockUntil = 0f;      // optional timed lock
//    private bool facingRight = true;

//    void Awake()
//    {
//        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
//    }

//    void Update()
//    {
//        // timed unlock
//        if (locked && lockUntil > 0f && Time.unscaledTime >= lockUntil)
//            locked = false;
//    }

//    /// <summary>Update facing from a movement vector. Ignored if locked.</summary>
//    public void SetMovementDir(Vector2 dir)
//    {
//        if (locked) return;
//        Apply(dir);
//    }

//    /// <summary>Lock facing toward this attack direction. Optional timed lock.</summary>
//    public void BeginAttackFacing(Vector2 dir, float lockSeconds = 0f)
//    {
//        Apply(dir);
//        locked = true;
//        lockUntil = lockSeconds > 0f ? Time.unscaledTime + lockSeconds : 0f;
//    }

//    /// <summary>Unlock facing so movement can flip again.</summary>
//    public void EndAttackFacing()
//    {
//        locked = false;
//        lockUntil = 0f;
//    }

//    //private void Apply(Vector2 dir)
//    //{
//    //    // only horizontal (left/right) matters
//    //    if (Mathf.Abs(dir.x) < xDeadzone) return;

//    //    bool shouldFaceRight = dir.x > 0f;
//    //    if (invert) shouldFaceRight = !shouldFaceRight;

//    //    // convention: sprite.flipX = true means face left (if your art faces right by default)
//    //    sprite.flipX = !shouldFaceRight;
//    //    facingRight = shouldFaceRight;
//    //}
//    private void Apply(Vector2 dir)
//    {
//        // only horizontal matters
//        if (Mathf.Abs(dir.x) < xDeadzone) return;

//        bool wantRight = dir.x > 0f;

//        // If your art faces RIGHT when flipX = false (most common), leave this true.
//        // If your art was authored facing LEFT by default, set this to false.
//        const bool artFacesRightByDefault = true;

//        // Optional user override from the Inspector
//        if (invert) wantRight = !wantRight;

//        if (artFacesRightByDefault)
//            sprite.flipX = !wantRight;   // right ¨ flipX=false, left ¨ flipX=true
//        else
//            sprite.flipX = wantRight;    // right ¨ flipX=true,  left ¨ flipX=false
//    }

//    public bool IsFacingRight => facingRight;
//    public bool IsLocked => locked;
//}


//using UnityEngine;

//public class Facing2D : MonoBehaviour
//{
//    [Header("What to flip")]
//    [Tooltip("The transform that visually represents the enemy (child with SpriteRenderer).")]
//    public Transform graphicsRoot;         // assign your sprite child here

//    [Header("Art Defaults")]
//    [Tooltip("Does your sprite artwork face RIGHT when scale.x is positive? Check this if yes.")]
//    public bool artFacesRightByDefault = true;

//    [Tooltip("Invert the computed facing. Use if your prefab hierarchy already mirrors X.")]
//    public bool invert = false;

//    [Header("Jitter Control")]
//    [Range(0f, 0.25f)] public float xDeadzone = 0.05f;

//    private Vector3 baseScale;
//    private bool locked;
//    private float lockUntil;
//    private bool facingRight = true;

//    void Awake()
//    {
//        if (!graphicsRoot)
//        {
//            // Try to auto-find the first SpriteRenderer in children
//            var sr = GetComponentInChildren<SpriteRenderer>();
//            if (sr) graphicsRoot = sr.transform;
//            else graphicsRoot = transform; // fallback
//        }
//        baseScale = graphicsRoot.localScale;
//    }

//    void Update()
//    {
//        // timed unlock using unscaled time (so hitstop doesn't break it)
//        if (locked && lockUntil > 0f && Time.unscaledTime >= lockUntil)
//        {
//            locked = false;
//            lockUntil = 0f;
//        }
//    }

//    /// <summary>Called from movement code. Ignored if currently locked.</summary>
//    public void SetMovementDir(Vector2 worldDir)
//    {
//        if (locked) return;
//        Apply(worldDir);
//    }

//    /// <summary>Lock facing toward attack dir for a duration (prevents mid-dash flips).</summary>
//    public void BeginAttackFacing(Vector2 worldDir, float lockSeconds = 0f)
//    {
//        Apply(worldDir);
//        locked = true;
//        lockUntil = (lockSeconds > 0f) ? Time.unscaledTime + lockSeconds : 0f;
//    }

//    public void EndAttackFacing()
//    {
//        locked = false;
//        lockUntil = 0f;
//    }

//    private void Apply(Vector2 worldDir)
//    {
//        if (Mathf.Abs(worldDir.x) < xDeadzone) return;

//        bool wantRight = worldDir.x > 0f;

//        // Adjust for art default and any requested inversion
//        // Target "positive visual right" = (artFacesRightByDefault ? +baseScale.x : -baseScale.x)
//        bool finalRight = wantRight;
//        if (!artFacesRightByDefault) finalRight = !finalRight;
//        if (invert) finalRight = !finalRight;

//        float sign = finalRight ? +1f : -1f;

//        // Set localScale.x deterministically (no accumulation)
//        graphicsRoot.localScale = new Vector3(
//            Mathf.Abs(baseScale.x) * sign,
//            baseScale.y,
//            baseScale.z
//        );

//        facingRight = finalRight;
//    }

//    public bool IsFacingRight => facingRight;
//    public bool IsLocked => locked;
//}

using UnityEngine;

public class Facing2D : MonoBehaviour
{
    [Header("Refs")]
    public SpriteRenderer sprite;        // if null, auto-find on this GameObject

    [Header("Art Defaults")]
    [Tooltip("If your art's default (flipX=false) visually faces RIGHT, keep this checked.")]
    public bool artFacesRightByDefault = true;

    [Tooltip("Invert facing if it still looks backwards (covers odd prefab setups).")]
    public bool invert = false;

    [Header("Jitter Control")]
    [Range(0f, 0.25f)] public float xDeadzone = 0.05f;

    // state
    private bool locked = false;
    private float lockUntil = 0f;
    private bool facingRight = true;

    void Awake()
    {
        if (!sprite) sprite = GetComponent<SpriteRenderer>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        // IMPORTANT: do NOT change transform.localScale on root; we only use sprite.flipX
    }

    void Update()
    {
        // timed unlock (unscaled so hitstop won't freeze it)
        if (locked && lockUntil > 0f && Time.unscaledTime >= lockUntil)
        {
            locked = false;
            lockUntil = 0f;
        }
    }

    /// Face according to movement direction (ignored if currently locked).
    public void SetMovementDir(Vector2 worldDir)
    {
        if (locked) return;
        Apply(worldDir);
    }

    /// Lock facing to an attack direction for a duration (prevents mid-dash flips).
    public void BeginAttackFacing(Vector2 worldDir, float lockSeconds = 0f)
    {
        Apply(worldDir);
        locked = true;
        lockUntil = lockSeconds > 0f ? Time.unscaledTime + lockSeconds : 0f;
    }

    public void EndAttackFacing()
    {
        locked = false;
        lockUntil = 0f;
    }

    private void Apply(Vector2 worldDir)
    {
        if (!sprite) return;
        if (Mathf.Abs(worldDir.x) < xDeadzone) return;

        bool wantRight = worldDir.x > 0f;

        // Map intent ¨ visual, accounting for art default & optional invert.
        bool finalRight = wantRight;
        if (!artFacesRightByDefault) finalRight = !finalRight;
        if (invert) finalRight = !finalRight;

        // Convention: flipX=true usually means visually LEFT when art faces RIGHT by default.
        sprite.flipX = !finalRight;
        facingRight = finalRight;
    }

    public bool IsFacingRight => facingRight;
    public bool IsLocked => locked;
}
