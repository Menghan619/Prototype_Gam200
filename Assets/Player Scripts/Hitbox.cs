using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    PolygonCollider2D poly;
    ContactFilter2D filter;
    readonly Collider2D[] buffer = new Collider2D[32]; // reuse this

    void Awake()
    {
        poly = GetComponent<PolygonCollider2D>();

        // Start from no filter, then tweak if you want.
        filter = new ContactFilter2D();
        filter.NoFilter();
        // Example filters:
        // filter.useTriggers = true;                      // include triggers
        // filter.SetLayerMask(LayerMask.GetMask("Enemy")); // only Enemy layer
    }

    void Update()
    {
        int count = poly.Overlap(filter, buffer); // <-- returns INT
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = buffer[i];
            if (hit == null || hit == poly) continue; // skip self, just in case
            Debug.Log("Overlapping: " + hit.name);
            
        }
        

    }

}
