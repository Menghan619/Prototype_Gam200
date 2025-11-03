//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class Hitbox : MonoBehaviour
//{

//    private void OnEnable()
//    {
//        hitThisWindow.Clear();
//    }
//    [SerializeField] float windowSeconds = 0.15f;
//    [SerializeField] LayerMask enemyLayers;
//    [SerializeField] int damage = 10;
//    [SerializeField] string DamageElement;

//    readonly HashSet<int> hitThisWindow = new HashSet<int>();
//    bool windowOpen;
//    [SerializeField] GameObject PlayerObject;
//    Transform Ptransform;

//    private void Start()
//    {
//        Ptransform = PlayerObject.transform;
//    }
//    void OnTriggerEnter2D(Collider2D other)
//    {

//        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;

//        int id = other.GetInstanceID();

//        if (!hitThisWindow.Add(id)) return; // already hit during this swing
//        // Get the integer layer value of the current GameObject
//        int layerNumber = other.gameObject.layer;

//        // Convert the layer number to its name
//        string layerName = LayerMask.LayerToName(layerNumber);

//        // Print the layer name to the console
//        Debug.Log("GameObject '" + other.gameObject.name + "' is on Layer: " + layerName);


//            Debug.Log(other.gameObject.name + "Got HIT");
//            other.gameObject.GetComponent<Enemy>().OldDamaged(layerName, Ptransform);





//    }
//    private void OnDisable()
//    {
//        hitThisWindow.Clear();
//    }
//}
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    [Header("Hit Settings")]
    public float damage = 1f;
    public Element damageElement = Element.Neutral;
    public float knockbackForce = 4.0f;

    [Header("Targeting")]
    public LayerMask enemyLayers;
    public Transform attacker; // assign the player/owner

    private readonly HashSet<Collider2D> hitThisWindow = new();

    private void OnEnable() { hitThisWindow.Clear(); }
    private void OnDisable() { hitThisWindow.Clear(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;
        if (!hitThisWindow.Add(other)) return; // already hit this window

        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        Vector2 dir = (other.transform.position - (attacker ? attacker.position : transform.position)).normalized;
        var packet = new DamagePacket(damage, damageElement, attacker, dir, knockbackForce);

        enemy.Damaged(packet);
        // Debug.Log($"{other.name} got HIT by {damageElement} for {damage}");
    }
}