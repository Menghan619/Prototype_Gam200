// CountKillOnDeath.cs
using UnityEngine;

[RequireComponent(typeof(Health))]
public class CountKillOnDeath : MonoBehaviour
{
    private Health hp;

    void Awake()
    {
        hp = GetComponent<Health>();
        // Health raises OnDeath without destroying the object. :contentReference[oaicite:3]{index=3}
        hp.OnDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath()
    {
        GameFlowManager.Instance?.ReportEnemyKilled();
        // Let your existing death handler (e.g., anim/explosion) destroy after.
    }
}
