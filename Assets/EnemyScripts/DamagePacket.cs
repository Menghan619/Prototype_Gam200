using UnityEngine;

public struct DamagePacket
{
    public float baseDamage;
    public Element attackElement;
    public Transform source;
    public Vector2 knockbackDir;
    public float knockbackForce;

    public DamagePacket(float baseDamage, Element attackElement, Transform source,
                        Vector2 knockbackDir, float knockbackForce)
    {
        this.baseDamage = baseDamage;
        this.attackElement = attackElement;
        this.source = source;
        this.knockbackDir = knockbackDir;
        this.knockbackForce = knockbackForce;
    }
}
