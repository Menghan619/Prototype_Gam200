using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 30f;
    public Element defenseElement = Element.Neutral;

    public event System.Action OnDeath;
    public bool IsDead { get; private set; }//

    public float CurrentHP { get; private set; }

    public Element LastHitElement { get; private set; } = Element.Neutral;

    public GameObject FloatingHitNumber;

    private void Awake() => CurrentHP = maxHP;

    /// <summary>
    /// Applies damage with element multipliers.
    /// Returns true if HP was reduced (i.e., multiplier > 0 and >0 final damage).
    /// </summary>
    public bool ApplyDamage(DamagePacket packet, out float finalDamage)
    {
        LastHitElement = packet.attackElement;
        float mult = ElementChart.GetMultiplier(packet.attackElement, defenseElement);
        if (mult <= 0f) // immune
        {
            finalDamage = 0f;
            return false;
        }

        finalDamage = packet.baseDamage * mult;

        if (finalDamage <= 0f) return false;
        Debug.Log("Final damage is:" +finalDamage);
        GameObject DamageNum = Instantiate(FloatingHitNumber, transform.position, Quaternion.identity) as GameObject; 
        DamageNum.transform.GetChild(0).GetComponent<TextMeshPro>().text = finalDamage.ToString();
        CurrentHP -= finalDamage;
        if (CurrentHP <= 0f)
        {
            //// Simple death handling for now
            //Destroy(gameObject);
            IsDead = true;
            OnDeath?.Invoke();   // let listeners (like WaterDemonDeathExplode) handle the sequence
                                 // DO NOT Destroy() here.
        }
        return true;
    }
    public void DestroyNow() => Destroy(gameObject);
}
