using UnityEngine;
using System.Collections.Generic;

public class EnemyStatusController : MonoBehaviour
{
    private readonly List<SlowBuff> slows = new();
    private float cachedMultiplier = 1f;

    struct SlowBuff { public float mult; public float until; }

    public float CurrentSpeedMultiplier
    {
        get
        {
            float now = Time.time;
            slows.RemoveAll(s => s.until <= now);
            float m = 1f;
            foreach (var s in slows) m *= s.mult;
            cachedMultiplier = Mathf.Clamp(m, 0.2f, 1f); // cap
            return cachedMultiplier;
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp(multiplier, 0.2f, 1f);
        slows.Add(new SlowBuff { mult = multiplier, until = Time.time + duration });
    }
}
