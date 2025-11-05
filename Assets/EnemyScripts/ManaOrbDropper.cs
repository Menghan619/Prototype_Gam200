using UnityEngine;

[RequireComponent(typeof(Health))]
public class ManaOrbDropper : MonoBehaviour
{
    public GameObject manaOrbPrefab;
    public int minOrbs = 1;
    public int maxOrbs = 2;
    public Element enemyDefenseElement = Element.Neutral; // or read from Enemy/Health if you expose it

    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        if (health) health.OnDeath += OnDead;
        var e = GetComponent<Enemy>();
        if (e) enemyDefenseElement = e.defenseElement;
    }
    void OnDestroy()
    {
        if (health) health.OnDeath -= OnDead;
    }

    void OnDead()
    {
        if (!manaOrbPrefab) return;

        Element weak = WeakAgainst(enemyDefenseElement);
        if (health.LastHitElement != weak) return;

        int count = Random.Range(minOrbs, maxOrbs + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 jitter = Random.insideUnitCircle * 0.25f;
            Instantiate(manaOrbPrefab, transform.position + (Vector3)jitter, Quaternion.identity);
        }
    }

    // Fire <- Water, Water <- Wind, Wind <- Fire  (your triangle)
    public static Element WeakAgainst(Element defense)
    {
        switch (defense)
        {
            case Element.Fire: return Element.Water; // Water beats Fire
            case Element.Water: return Element.Wind;  // Wind beats Water
            case Element.Wind: return Element.Fire;  // Fire beats Wind
            default: return Element.Neutral;
        }
    }
}
