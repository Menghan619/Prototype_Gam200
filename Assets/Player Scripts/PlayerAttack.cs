using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private StringBuilder KeyInputs;
    private bool isAttacking;
    public Animator SlashAnime;
    private Abilities AbilitiesScript;
    
    // Update is called once per frame

    private void Start()
    {
        
        isAttacking = false;
        KeyInputs = new StringBuilder();
        AbilitiesScript = GetComponent<Abilities>();
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {

            switch (KeyInputs.Length)
            {

                case 0:
                    KeyInputs.Append(Input.inputString);
                    break;

                case 1:
                    KeyInputs.Append(Input.inputString);
                    string InputFinal = KeyInputs.ToString().ToUpper();
                    Debug.Log(KeyInputs.ToString().ToUpper());
                    if (InputFinal == "QQ")
                    {

                        AbilitiesScript.AttackQQ();
                        KeyInputs.Clear();
                        
                    }
                    else
                    {
                        KeyInputs.Clear();
                    }
                    break;

                     

            }
            
        }
                

     }
    //public void AttackQQ()
    //{
    //    SlashAnime.SetTrigger("QQ");

    //}

}

     

    



