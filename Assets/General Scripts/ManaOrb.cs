using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ManaOrb : MonoBehaviour
{
    public int amount = 12;
    public float magnetRange = 4.5f;
    public float magnetSpeed = 10f;
    public float idleFloatAmplitude = 0.1f;
    public float idleFloatFreq = 3f;

    Transform player;
    PlayerMana playerMana;
    Vector3 basePos;

    void Awake()
    {
        basePos = transform.position;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p)
        {
            player = p.transform;
            playerMana = p.GetComponent<PlayerMana>();
        }
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        // small idle bob
        transform.position = new Vector3(
            transform.position.x,
            basePos.y + Mathf.Sin(Time.time * idleFloatFreq) * idleFloatAmplitude,
            transform.position.z
        );

        if (!player) return;

        float d = Vector2.Distance(player.position, transform.position);
        if (d <= magnetRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * magnetSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!playerMana) return;
        if (!other.CompareTag("Player")) return;

        playerMana.Add(amount);
        // TODO: play pickup VFX/SFX
        Destroy(gameObject);
    }
}
