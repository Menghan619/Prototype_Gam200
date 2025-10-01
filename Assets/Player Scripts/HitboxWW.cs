using System.Collections.Generic;
using UnityEngine;

public class HitboxWW : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    [SerializeField] float windowSeconds = 0.15f;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] int damage = 10;

    readonly HashSet<int> hitThisWindow = new HashSet<int>();
    bool windowOpen;

     private void OnEnable()
    {
        hitThisWindow.Clear();
    }
    void OnTriggerEnter2D(Collider2D other)
    {

        //if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;

        int id = other.GetInstanceID();

        if (!hitThisWindow.Add(id)) return; // already hit during this swing
        // Get the integer layer value of the current GameObject
        int layerNumber = other.gameObject.layer;

        // Convert the layer number to its name
        string layerName = LayerMask.LayerToName(layerNumber);

        // Print the layer name to the console
        Debug.Log("GameObject '" + other.gameObject.name + "' is on Layer: " + layerName);

        if (layerName == "EnemyW")
        {
            Debug.Log(other.gameObject.name + "Got HIT");
            other.gameObject.GetComponent<Enemy>().Damaged();
        }
        else
        {
            Debug.Log(other.gameObject.name + "NO DAMAGE");
        }
    }
}
