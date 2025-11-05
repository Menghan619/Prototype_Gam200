//using UnityEngine;
//using System;

//public class PlayerMana : MonoBehaviour
//{
//    [Header("Mana")]
//    public int maxMana = 100;
//    public int startMana = 100;
//    public float regenPerSecond = 7f;     // tune to taste
//    public bool regenWhileCasting = true; // optional

//    public int CurrentMana { get; private set; }

//    public event Action<int, int> OnManaChanged; // (current, max)

//    void Awake()
//    {
//        CurrentMana = Mathf.Clamp(startMana, 0, maxMana);
//        OnManaChanged?.Invoke(CurrentMana, maxMana);
//    }

//    void Update()
//    {
//        if (regenPerSecond > 0f && (regenWhileCasting || !castingLock))
//        {
//            float delta = regenPerSecond * Time.deltaTime;
//            if (delta > 0f && CurrentMana < maxMana)
//            {
//                CurrentMana = Mathf.Min(maxMana, Mathf.RoundToInt(CurrentMana + delta));
//                OnManaChanged?.Invoke(CurrentMana, maxMana);
//            }
//        }
//    }

//    // Optional: Abilities can set this true during long windups if you want to pause regen
//    bool castingLock = false;
//    public void SetCastingLock(bool v) => castingLock = v;

//    public bool CanAfford(int cost) => cost <= 0 || CurrentMana >= cost;

//    public bool Spend(int cost)
//    {
//        if (cost <= 0) return true;
//        if (CurrentMana < cost) return false;
//        CurrentMana -= cost;
//        OnManaChanged?.Invoke(CurrentMana, maxMana);
//        return true;
//    }

//    public void Add(int amount)
//    {
//        if (amount <= 0) return;
//        CurrentMana = Mathf.Min(maxMana, CurrentMana + amount);
//        OnManaChanged?.Invoke(CurrentMana, maxMana);
//    }

//    public void SetMax(int newMax, bool refill = false)
//    {
//        maxMana = Mathf.Max(1, newMax);
//        if (refill) CurrentMana = maxMana;
//        CurrentMana = Mathf.Clamp(CurrentMana, 0, maxMana);
//        OnManaChanged?.Invoke(CurrentMana, maxMana);
//    }
//}

using UnityEngine;
using System;
public class PlayerMana : MonoBehaviour
{
    public int maxMana = 100;
    public int startMana = 100;
    public float regenPerSecond = 7f;
    public bool regenWhileCasting = true;

    public int CurrentMana { get; private set; }
    public event System.Action<int, int> OnManaChanged;

    // NEW: carry the fractional regen between frames
    float regenCarry;
    bool castingLock = false;

    void Awake()
    {
        CurrentMana = Mathf.Clamp(startMana, 0, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    void Update()
    {
        if (regenPerSecond <= 0f) return;
        if (!regenWhileCasting && castingLock) return;
        if (CurrentMana >= maxMana) return;

        // Use unscaled so regen isn’t paused by hitstop/timeScale=0
        float add = regenPerSecond * Time.unscaledDeltaTime;
        regenCarry += add;

        if (regenCarry >= 1f)
        {
            int inc = Mathf.FloorToInt(regenCarry);
            regenCarry -= inc;
            int old = CurrentMana;
            CurrentMana = Mathf.Min(maxMana, CurrentMana + inc);
            if (CurrentMana != old) OnManaChanged?.Invoke(CurrentMana, maxMana);
        }
    }

    public void SetCastingLock(bool v) => castingLock = v;

    public bool CanAfford(int cost) => cost <= 0 || CurrentMana >= cost;

    public bool Spend(int cost)
    {
        if (cost <= 0) return true;
        if (CurrentMana < cost) return false;
        CurrentMana -= cost;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        CurrentMana = Mathf.Min(maxMana, CurrentMana + amount);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }
}
