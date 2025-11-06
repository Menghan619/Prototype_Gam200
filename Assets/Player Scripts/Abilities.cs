using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections;

public class Abilities : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator SlashAnimes;
    public Animator CharSlashAnimes;
    [SerializeField] GameObject AbilityQQ;
    [SerializeField] GameObject AbilityWW;
    [SerializeField] GameObject AbilityWE;
    [SerializeField] private GameObject AbilityEE_FireBase;  // enable collider window for EE
    [SerializeField] private GameObject SteamBurstFloorPrefab; // instantiate floor DoT on WE/EW
    [SerializeField] private GameObject AbilityQW_SteamBurst;
    [SerializeField] private GameObject SteamBustLocation;
    [SerializeField] private GameObject AbilityLMB;

    [SerializeField] private float hitWindow = 0.05f; // seconds the hitbox is open

    [SerializeField] private PlayerMovement movement;


    private Collider2D hitboxQQ; // use Collider2D for 2D
    private Collider2D hitboxWW;
    private Collider2D hitboxWE;
    private Collider2D hitboxEE;
    private Collider2D hitboxQW_SteamBurst;
    private Collider2D hitboxLMB;
    // Update is called once per frame

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float dashDistance = 4f;   // how far to dash
    [SerializeField] private float dashSpeed = 18f;     // units/sec
    [SerializeField] private LayerMask dashBlockers;    // walls/obstacles to stop at

    private Rigidbody2D rb;
    private float originalGravity; // if you use gravity on the rb (often 0 in 2D top-down)

    AudioManager audioManager;

    private PlayerMana mana;
    [SerializeField] private AbilityCosts costs;

    [Header("Player Dash Manager")]
    [SerializeField] private float evadeDashDistance = 4f;
    [SerializeField] private float evadeDashSpeed = 22f;
    [SerializeField] private float evadeCooldown = 1.0f;
    private float nextEvadeReady = 0f;

    [SerializeField] private float evadeIFrameDuration = 1f;  // tweak to taste
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (AbilityQQ != null)
            hitboxQQ = AbilityQQ.GetComponent<Collider2D>(); // or Collider2D
        if (AbilityWW != null)
            hitboxWW = AbilityWW.GetComponent<Collider2D>(); // or Collider2D
        if (AbilityWE != null)
            hitboxWE = AbilityWE.GetComponent<Collider2D>(); // or Collider2D
        if (AbilityEE_FireBase) hitboxEE = AbilityEE_FireBase.GetComponent<Collider2D>();
        if (AbilityQW_SteamBurst) hitboxQW_SteamBurst = AbilityQW_SteamBurst.GetComponent<Collider2D>();
        if (AbilityLMB) hitboxLMB = AbilityLMB.GetComponent<Collider2D>();
        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();

        // NEW:
        if (!mana) mana = GetComponent<PlayerMana>();
        if (!rb) rb = GetComponent<Rigidbody2D>();

        if (!playerHealth) playerHealth = GetComponent<PlayerHealth>();

        originalGravity = rb ? rb.gravityScale : 0f;
    }
    private void Start()
    {
        if (AbilityQQ != null) AbilityQQ.SetActive(false);
        if (hitboxQQ != null) hitboxQQ.enabled = false; // collider closed
        if (AbilityWW != null) AbilityWW.SetActive(false);
        if (hitboxWW != null) hitboxWW.enabled = false; // collider closed
        if (AbilityWE != null) AbilityWE.SetActive(false);
        if (hitboxWE != null) hitboxWE.enabled = false; // collider closed
        if (AbilityEE_FireBase) AbilityEE_FireBase.SetActive(false);
        if (hitboxEE) hitboxEE.enabled = false;
        if (AbilityQW_SteamBurst) AbilityQW_SteamBurst.SetActive(false);
        if (hitboxQW_SteamBurst) hitboxQW_SteamBurst.enabled = false;
        if (AbilityLMB) AbilityLMB.SetActive(false);
        if (hitboxLMB) hitboxLMB.enabled = false;


    }
    

    public void AttackAbility(string AbilityUsed)
    {
        StopAllCoroutines();
        FaceTowardsMouse();

        switch (AbilityUsed)
        {
            case "QQ":
                SlashAnimes.SetTrigger("WaterSlash");
                audioManager.PlaySFX(audioManager.WaterSlash);
                StartCoroutine(OpenHitboxWindow("QQ"));

                break;
            case "WW":
                StartCoroutine(DashThenSlash());
                break;
            case "EE": // 🔥 Fire Slash (EE)
                CharSlashAnimes.SetTrigger("FireSlash");        // placeholder
                audioManager.PlaySFX(audioManager.ComboSlash);  // or FireSlash SFX
                StartCoroutine(OpenHitboxWindow("EE"));
                break;
            case "WE": // 🔥🌀 Fire Cyclone (FW/WF) — move your current cyclone logic here
                CharSlashAnimes.SetTrigger("FireCyclone");      // placeholder
                audioManager.PlaySFX(audioManager.ComboSlash);
                StartCoroutine(OpenHitboxWindow("WE"));         // if using a timed collider like your old WE
                break;
            case "QE": // ☁️♨️ Steam Burst (WE/EW) — spawn floor that applies slow+dot
                CharSlashAnimes.SetTrigger("SteamBurst");       // placeholder
                audioManager.PlaySFX(audioManager.ComboSlash);
                StartCoroutine(OpenHitboxWindow("QE"));
                StartCoroutine(DoSteamBurst());                 // new coroutine below
                break;
            case "LMB": // ☁️♨️ Steam Burst (WE/EW) — spawn floor that applies slow+dot
                CharSlashAnimes.SetTrigger("WaterAttack");       // placeholder
                audioManager.PlaySFX(audioManager.ComboSlash);
                StartCoroutine(OpenHitboxWindow("LMB"));
                            
                break;

        }
       
                           // cancel any previous swing
        




    }

    private IEnumerator OpenHitboxWindow(string AbilityUsed)
    {
        // Open
        switch (AbilityUsed)
        {
            case "QQ":

                int cost = costs ? costs.WaterQ : 20;
                if (mana && !mana.Spend(cost)) yield break;

                if (movement != null)
                {
                    movement.IsInputLocked = true; // ignore new clicks
                    movement.Stop();               // cancels current move
                    movement.enabled = false;      // optional: fully disable Update() while dashing
                }
                AbilityQQ.SetActive(true);
                if (hitboxQQ != null) hitboxQQ.enabled = true;

                // Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
                yield return new WaitForSeconds(hitWindow);

                // Close
                if (hitboxQQ != null) hitboxQQ.enabled = false;
                AbilityQQ.SetActive(false);

                if (movement != null)
                {
                    movement.enabled = true;
                    movement.IsInputLocked = false;
                    // Do NOT call SetDestination — player will remain where the dash ended
                }
                break;
            case "WW":
                if (movement != null)
                {
                    movement.IsInputLocked = true; // ignore new clicks
                    movement.Stop();               // cancels current move
                    movement.enabled = false;      // optional: fully disable Update() while dashing
                }
                AbilityWW.SetActive(true);
                if (hitboxWW != null) hitboxWW.enabled = true;

                // Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
                yield return new WaitForSeconds(hitWindow);

                // Close
                if (hitboxWW != null) hitboxWW.enabled = false;
                AbilityWW.SetActive(false);

                if (movement != null)
                {
                    movement.enabled = true;
                    movement.IsInputLocked = false;
                    // Do NOT call SetDestination — player will remain where the dash ended
                }
                break;
            case "EE": // FireBase window
                {
                    cost = costs ? costs.FireE : 18;
                    if (mana && !mana.Spend(cost)) yield break;

                    if (movement) { movement.IsInputLocked = true; movement.Stop(); movement.enabled = false; }
                    AbilityEE_FireBase.SetActive(true);
                    if (hitboxEE) hitboxEE.enabled = true;

                    yield return new WaitForSeconds(hitWindow);

                    if (hitboxEE) hitboxEE.enabled = false;
                    AbilityEE_FireBase.SetActive(false);
                    if (movement) { movement.enabled = true; movement.IsInputLocked = false; }
                    break;
                }

            case "WE": // (Only if you keep cyclone as a timed collider; if cyclone is a spawned prefab, spawn here instead)
                {
                    cost = costs ? (costs.FireCyclone_FW > 0 ? costs.FireCyclone_FW : costs.SteamBurst_WE) : 45;
                    if (mana && !mana.Spend(cost)) yield break;

                    if (movement) { movement.IsInputLocked = true; movement.Stop(); movement.enabled = false; }

                    // Reuse your previous cyclone object (rename the old WE hitbox to FireCyclone and enable it here)
                    // Example:
                    // FireCycloneGO.SetActive(true); FireCycloneCollider.enabled = true;
                    // yield return new WaitForSeconds(cycloneDuration);
                    // FireCycloneCollider.enabled = false; FireCycloneGO.SetActive(false);

                    yield return new WaitForSeconds(hitWindow); // placeholder if you keep the same pattern

                    if (movement) { movement.enabled = true; movement.IsInputLocked = false; }
                    break;
                }
            case "QE":

                // PAY HERE (before locking movement / enabling hitbox)
                cost = costs ? costs.SteamBurst_WE : 45;
                if (mana && !mana.Spend(cost)) yield break;

                if (movement != null)
                {
                    movement.IsInputLocked = true; // ignore new clicks
                    movement.Stop();               // cancels current move
                    movement.enabled = false;      // optional: fully disable Update() while dashing
                }
                AbilityQW_SteamBurst.SetActive(true);
                if (AbilityQW_SteamBurst != null) hitboxQW_SteamBurst.enabled = true;

                // Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
                yield return new WaitForSeconds(hitWindow);

                // Close
                if (hitboxQW_SteamBurst != null) hitboxQW_SteamBurst.enabled = false;
                AbilityQW_SteamBurst.SetActive(false);

                if (movement != null)
                {
                    movement.enabled = true;
                    movement.IsInputLocked = false;
                    // Do NOT call SetDestination — player will remain where the dash ended
                }
                break;

            case "LMB":

                // LMB IS FREE, NO MANA COST
                
                

                if (movement != null)
                {
                    movement.IsInputLocked = true; // ignore new clicks
                    movement.Stop();               // cancels current move
                    movement.enabled = false;      // optional: fully disable Update() while dashing
                }
                AbilityLMB.SetActive(true);
                if (AbilityLMB != null) hitboxLMB.enabled = true;

                // Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
                yield return new WaitForSeconds(hitWindow);

                // Close
                if (hitboxLMB != null) hitboxLMB.enabled = false;
                AbilityLMB.SetActive(false);

                if (movement != null)
                {
                    movement.enabled = true;
                    movement.IsInputLocked = false;
                    // Do NOT call SetDestination — player will remain where the dash ended
                }
                break;

        }
        //AbilityQQ.SetActive(true);
        //if (hitbox != null) hitbox.enabled = true;

        //// Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
        //yield return new WaitForSeconds(hitWindow);

        //// Close
        //if (hitbox != null) hitbox.enabled = false;
        //AbilityQQ.SetActive(false);
    }

    // Animation Event: call when the swing ends
    //public void ColliderOff()
    //{
    //    if (hitbox != null) hitbox.enabled = false;
    //    if (AbilityQQ != null) AbilityQQ.SetActive(false);
    //}
    void FaceTowardsMouse()
    {
        
        float screenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ));

        //spriteRenderer.flipY = (mouseWorld.y < transform.position.y);
        if (mouseWorld.x < spriteRenderer.transform.position.x)
        {
            // Face left
            spriteRenderer.transform.localScale = new Vector3(1, -1, 1);
        }
        else if (mouseWorld.x > spriteRenderer.transform.position.x)
        {
            // Face right
            spriteRenderer.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private IEnumerator DashThenSlash()
    {
        int cost = costs ? costs.WindW : 20;
        if (mana && !mana.Spend(cost)) yield break;

        if (movement != null)
        {
            movement.IsInputLocked = true; // ignore new clicks
            movement.Stop();               // cancels current move
            movement.enabled = false;      // optional: fully disable Update() while dashing
        }


        // Get mouse world & dash direction
        float z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right; // fallback

        // Compute dash target; stop early if something blocks us
        float maxDist = dashDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dashDistance, dashBlockers);
        if (hit.collider != null)
            maxDist = Mathf.Max(0f, hit.distance - 0.05f); // small skin

        Vector2 start = transform.position;
        Vector2 target = start + dir * maxDist;

        // Optional: temporary physics changes for clean movement
        Vector2 savedVel = Vector2.zero;
        if (rb != null)
        {
            savedVel = rb.linearVelocity;
            rb.gravityScale = 0f; // top-down usually 0 anyway
        }
        audioManager.PlaySFX(audioManager.DashSFX);
        // Dash move loop
        if (rb != null)
        {
            while (Vector2.Distance(rb.position, target) > 0.02f)
            {
                Vector2 next = Vector2.MoveTowards(rb.position, target, dashSpeed * Time.fixedDeltaTime);
                rb.MovePosition(next);
                yield return new WaitForFixedUpdate();
            }
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = originalGravity;
            
        }
        else
        {
            while (Vector2.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, dashSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // Now unleash the slash
        SlashAnimes.SetTrigger("WindSlash");
        audioManager.PlaySFX(audioManager.WindSlash);
        yield return StartCoroutine(OpenHitboxWindow("WW"));

        // 3) Re-enable movement but keep destination cleared so we STAY at dash end
        if (movement != null)
        {
            movement.enabled = true;
            movement.IsInputLocked = false;
            // Do NOT call SetDestination — player will remain where the dash ended
        }
    }
    [SerializeField] private float steamCastWindup = 0.15f; // small delay before spawn

    private IEnumerator DoSteamBurst()
    {
        //int cost = costs ? costs.SteamBurst_WE : 45;
        //if (mana && !mana.Spend(cost)) yield break;

        if (movement) { movement.IsInputLocked = true; movement.Stop(); movement.enabled = false; }

        yield return new WaitForSeconds(steamCastWindup);

        if (SteamBurstFloorPrefab)
        {
            var go = Instantiate(SteamBurstFloorPrefab, SteamBustLocation.transform.position, Quaternion.identity);
            // Pass source (player) so DoT can attribute DamagePacket source for knockback frames etc.
            var dot = go.GetComponent<SteamBoilDoT>();
            if (dot) dot.Init(owner: this.transform);
        }

        if (movement) { movement.enabled = true; movement.IsInputLocked = false; }
    }


    public void DashEvade()
    {
        if (Time.time < nextEvadeReady) return;
        CharSlashAnimes.SetTrigger("Evade");
        StartCoroutine(CoDashEvade());
    }

    private IEnumerator CoDashEvade()
    {
        nextEvadeReady = Time.time + evadeCooldown;

        if (movement) { movement.IsInputLocked = true; movement.Stop(); movement.enabled = false; }

        // dash direction = towards mouse
        float z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        float maxDist = evadeDashDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, evadeDashDistance, dashBlockers);
        if (hit.collider) maxDist = Mathf.Max(0f, hit.distance - 0.05f);

        Vector2 target = (Vector2)transform.position + dir * maxDist;

        // Optional: i-frames hook later (PlayerHealth.SetInvulnerable(true))
        if (playerHealth) playerHealth.GrantTemporaryInvulnerability(evadeIFrameDuration, blink: false);


        audioManager.PlaySFX(audioManager.DashSFX);

        if (rb)
        {
            while (Vector2.Distance(rb.position, target) > 0.02f)
            {
                Vector2 next = Vector2.MoveTowards(rb.position, target, evadeDashSpeed * Time.fixedDeltaTime);
                rb.MovePosition(next);
                yield return new WaitForFixedUpdate();
            }
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
        }
        else
        {
            while (Vector2.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, evadeDashSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // Optional: end i-frames hook here (PlayerHealth.SetInvulnerable(false))

        if (movement) { movement.enabled = true; movement.IsInputLocked = false; }
    }



}
