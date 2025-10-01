using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5f;
    public float stopDistance = 0.05f;
    private Vector3 targetPos;
    private bool isMoving = false;
    private Rigidbody2D rb;
    public bool IsInputLocked { get; set; } = false;
    public Animator CharMoveAnim;

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // RECOMMENDED Rigidbody2D settings in Inspector:
        // Body Type = Dynamic
        // Collision Detection = Continuous
        // Freeze Rotation Z = ON (so bumps don�ft spin you)
        // Interpolate = Interpolate (smoother visuals)
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(1))
        //{

        //    Vector3 mousePos = Input.mousePosition;
        //    mousePos.z = Mathf.Abs(Camera.main.transform.position.z);//changing the z axis to follow the camera
        //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        //    targetPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        //    isMoving = true;

        //    float screenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        //    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ));

        //    spriteRenderer.flipX = (mouseWorld.x < transform.position.x);



        //}
        //if (isMoving)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        //    if(Vector3.Distance(transform.position, targetPos) < 0.05f)
        //    {
        //        isMoving = false;
        //    }
        //}

        // Only accept new clicks if not locked
        //if (!IsInputLocked && Input.GetMouseButtonDown(1))
        //{
        //    Vector3 mousePos = Input.mousePosition;
        //    mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        //    SetDestination(worldPos);
        //}

        //if (isMoving)
        //{
        //    CharMoveAnim.SetTrigger("Moving");
        //    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        //    if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        //    {
        //        isMoving = false;
        //        CharMoveAnim.SetTrigger("Idle");
        //    }
        //}

        // Only accept new clicks if not locked
        if (!IsInputLocked && Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            SetDestination(worldPos);
        }

        // Anim triggers (purely cosmetic; physics happens in FixedUpdate)
        if (isMoving) CharMoveAnim.SetTrigger("Moving");
        else CharMoveAnim.SetTrigger("Idle");
    }
    void FixedUpdate()
    {
        if (!isMoving)
        {
            rb.linearVelocity = Vector2.zero; // ensure we fully stop
            return;
        }

        // Physics-based movement with collisions
        Vector2 pos2D = rb.position;
        Vector2 toTarget = ((Vector2)targetPos - pos2D);
        float dist = toTarget.magnitude;

        if (dist <= stopDistance)
        {
            Debug.Log("Stopped");
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            Stop();
            return;
        }

        // Desired velocity toward the target
        Vector2 dir = toTarget / Mathf.Max(dist, 0.0001f);
        Vector2 desiredVel = dir * moveSpeed;

        // Option A: Velocity-driven movement (good for sliding along walls)
        //rb.linearVelocity = desiredVel;

        // Option B (alternative): kinematic-like stepping that still collides
         Vector2 step = desiredVel * Time.fixedDeltaTime;
         rb.MovePosition(pos2D + step);

        // Face toward motion without flipping children
        if (spriteRenderer != null)
        {
            // If nearly stopped, keep current flip; otherwise flip by x direction
            if (Mathf.Abs(desiredVel.x) > 0.001f)
                spriteRenderer.flipX = (desiredVel.x < 0f);
        }
    }
    public void SetDestination(Vector3 worldPos)
    {
        if (IsInputLocked) return;

        targetPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        isMoving = true;

        // Face towards destination without flipping children
        if (spriteRenderer != null)
            spriteRenderer.flipX = (worldPos.x < transform.position.x);
    }

    // NEW: hard stop + clear target (used before dash)
    public void Stop()
    {
        isMoving = false;
        targetPos = transform.position; // clear pending path
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Stop();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Stop();
    }
}
