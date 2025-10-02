using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //PolygonCollider2D poly;
    //ContactFilter2D filter;
    //readonly Collider2D[] buffer = new Collider2D[32]; // reuse this

    //[Header("Hit Window")]
    //[SerializeField] float windowSeconds = 0.15f;

    //[Header("Targeting")]
    //[SerializeField] LayerMask enemyLayers;


    //bool windowOpen;
    //Coroutine windowRoutine;

    //readonly HashSet<int> hitThisWindow = new HashSet<int>();
    //void Awake()
    //{
    //    poly = GetComponent<PolygonCollider2D>();

    //    // Start from no filter, then tweak if you want.
    //    filter = new ContactFilter2D();
    //    filter.NoFilter();
    //    // Example filters:
    //    // filter.useTriggers = true;                      // include triggers
    //    // filter.SetLayerMask(LayerMask.GetMask("Enemy")); // only Enemy layer
    //    filter.useTriggers = true; // usually your hitbox is a trigger; include other triggers too if needed
    //    filter.SetLayerMask(enemyLayers);
    //    filter.useLayerMask = true;
    //}

    //void Update()
    //{
    //    if (!windowOpen || poly == null) return;

    //    //int count = poly.Overlap(filter, buffer); // <-- returns INT
    //    int count = poly.OverlapCollider(filter, buffer);
    //    for (int i = 0; i < count; i++)
    //    {
    //        Collider2D hit = buffer[i];
    //        if (hit == null || hit == poly) continue; // skip self, just in case
    //        int id = hit.GetInstanceID();
    //        if (hitThisWindow.Contains(id)) continue;  // already processed this target during this swing
    //        hitThisWindow.Add(id);
    //        Debug.Log("Overlapping: " + hit.name);

    //    }


    //}
    //public void OpenWindow(float? overrideSeconds = null)
    //{
    //    if (windowRoutine != null) StopCoroutine(windowRoutine);
    //    windowRoutine = StartCoroutine(WindowRoutine(overrideSeconds ?? windowSeconds));
    //}

    //IEnumerator WindowRoutine(float seconds)
    //{
    //    hitThisWindow.Clear();         // reset dedupe for this swing
    //    windowOpen = true;
    //    // If you also want to “open” by enabling collider:
    //    // poly.enabled = true;

    //    yield return new WaitForSeconds(seconds);

    //    windowOpen = false;
    //    // poly.enabled = false;       // optional close
    //}

    //public void CloseWindowNow()
    //{
    //    if (windowRoutine != null) StopCoroutine(windowRoutine);
    //    windowOpen = false;
    //}
    private void OnEnable()
    {
        hitThisWindow.Clear();
    }
    [SerializeField] float windowSeconds = 0.15f;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] int damage = 10;

    readonly HashSet<int> hitThisWindow = new HashSet<int>();
    bool windowOpen;
    [SerializeField] GameObject PlayerObject;
    Transform Ptransform;

    private void Start()
    {
        Ptransform = PlayerObject.transform;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;

        int id = other.GetInstanceID();
        
        if (!hitThisWindow.Add(id)) return; // already hit during this swing
        // Get the integer layer value of the current GameObject
        int layerNumber = other.gameObject.layer;

        // Convert the layer number to its name
        string layerName = LayerMask.LayerToName(layerNumber);

        // Print the layer name to the console
        Debug.Log("GameObject '" + other.gameObject.name + "' is on Layer: " + layerName);
        
        
            Debug.Log(other.gameObject.name + "Got HIT");
            other.gameObject.GetComponent<Enemy>().Damaged(layerName, Ptransform);
        
       
         
            
        
    }
    private void OnDisable()
    {
        hitThisWindow.Clear();
    }
}
