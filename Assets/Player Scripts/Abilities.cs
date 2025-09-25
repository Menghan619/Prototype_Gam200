using UnityEngine;

public class Abilities : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Animator SlashAnimes;
    [SerializeField] GameObject AbilityQQ;
    // Update is called once per frame
    private void Start()
    {
        AbilityQQ.SetActive(false);
    }
    

    public void AttackQQ()
    {
        
        SlashAnimes.SetTrigger("QQ");
        AbilityQQ.SetActive(true);
        


    }
    
    public void collideroff(GameObject Ability)
    {
        Ability.SetActive(false);
    }
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
