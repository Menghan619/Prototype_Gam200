using UnityEngine;

public class Abilities : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator SlashAnimes;
    // Update is called once per frame
    void Update()
    {
        
    }
    public void AttackQQ()
    {
        
        SlashAnimes.SetTrigger("QQ");

    }
}
