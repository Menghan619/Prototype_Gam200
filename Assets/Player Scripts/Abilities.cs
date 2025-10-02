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
    [SerializeField] private float hitWindow = 0.05f; // seconds the hitbox is open

    [SerializeField] private PlayerMovement movement;


    private Collider2D hitboxQQ; // use Collider2D for 2D
    private Collider2D hitboxWW;
    private Collider2D hitboxWE;
    // Update is called once per frame

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float dashDistance = 4f;   // how far to dash
    [SerializeField] private float dashSpeed = 18f;     // units/sec
    [SerializeField] private LayerMask dashBlockers;    // walls/obstacles to stop at

    private Rigidbody2D rb;
    private float originalGravity; // if you use gravity on the rb (often 0 in 2D top-down)

    AudioManager audioManager;


    private void Awake()
    {
        if (AbilityQQ != null)
            hitboxQQ = AbilityQQ.GetComponent<Collider2D>(); // or Collider2D
        if (AbilityWW != null)
            hitboxWW = AbilityWW.GetComponent<Collider2D>(); // or Collider2D
        if (AbilityWE != null)
            hitboxWE = AbilityWE.GetComponent<Collider2D>(); // or Collider2D
        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();
    }
    private void Start()
    {
        if (AbilityQQ != null) AbilityQQ.SetActive(false);
        if (hitboxQQ != null) hitboxQQ.enabled = false; // collider closed
        if (AbilityWW != null) AbilityWW.SetActive(false);
        if (hitboxWW != null) hitboxWW.enabled = false; // collider closed
        if (AbilityWE != null) AbilityWE.SetActive(false);
        if (hitboxWE != null) hitboxWE.enabled = false; // collider closed
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
            case "WE":
                
                CharSlashAnimes.SetTrigger("Aoe");
                audioManager.PlaySFX(audioManager.ComboSlash);
                StartCoroutine(OpenHitboxWindow("WE"));
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


            case "WE":

                if (movement != null)
                {
                    movement.IsInputLocked = true; // ignore new clicks
                    movement.Stop();               // cancels current move
                    movement.enabled = false;      // optional: fully disable Update() while dashing
                }
                AbilityWE.SetActive(true);
                if (hitboxWE != null) hitboxWE.enabled = true;

                // Keep it open long enough for OnTriggerEnter/OnCollision callbacks in the hitbox script to run
                yield return new WaitForSeconds(hitWindow);

                // Close
                if (hitboxWE != null) hitboxWE.enabled = false;
                AbilityWE.SetActive(false);

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


}
