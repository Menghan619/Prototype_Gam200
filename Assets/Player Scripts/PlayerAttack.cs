using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private StringBuilder KeyInputs;
    private bool isAttacking;
    public Animator SlashAnime;
    public Animator CharSlashAnime;
    private Abilities AbilitiesScript;
    [SerializeField] GameObject AbilityQQ;
    private Vector3 targetPos;
    [SerializeField] GameObject ElementIndicator;
    SpriteRenderer elementInd;
    // Update is called once per frame
    [SerializeField] Sprite WindIcon;
    [SerializeField] Sprite WaterIcon;
    [SerializeField] Sprite FireIcon;

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float attackRate = 2f;
    float nextAttackTime = 0f;
    private void Start()
    {
        
        isAttacking = false;
        KeyInputs = new StringBuilder();
        AbilitiesScript = GetComponent<Abilities>();
        elementInd = ElementIndicator.GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (KeyInputs.Length >= 2)
        {
            KeyInputs.Clear();

        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) )
        {
            
            switch (KeyInputs.Length)
            {

                case 0:
                    
                    KeyInputs.Append(Input.inputString);
                    if (KeyInputs.ToString().ToUpper() == "Q")
                    {
                        elementInd.sprite = WaterIcon;
                    }else if (KeyInputs.ToString().ToUpper() == "W")
                    {
                        elementInd.sprite = WindIcon;
                    }
                    break;

                case 1:
                    KeyInputs.Append(Input.inputString);
                    string InputFinal = KeyInputs.ToString().ToUpper();
                    Debug.Log(KeyInputs.ToString().ToUpper());
                    if (InputFinal == "QQ")
                    {
                        elementInd.sprite = WaterIcon;
                        if (Time.time >= nextAttackTime) {
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("QQ");
                            CharSlashAnime.SetTrigger("WaterAttack");

                            nextAttackTime = Time.time + 1f / attackRate;
                            KeyInputs.Clear();
                        }
                        
                        
                        
                    }
                    else if (InputFinal == "WW")
                    {
                        elementInd.sprite = WindIcon;
                        if (Time.time >= nextAttackTime)
                        {
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("WW");
                            CharSlashAnime.SetTrigger("WindAttack");
                            nextAttackTime = Time.time + 1f / attackRate;
                            KeyInputs.Clear();
                        }
                       
                    }
                    else if (InputFinal == "QW" || InputFinal == "WQ")
                    {
                        elementInd.sprite = FireIcon;
                        //CharSlashAnime.SetTrigger("Aoe");
                        if (Time.time >= nextAttackTime)
                        {
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("WE");
                            KeyInputs.Clear();
                            nextAttackTime = Time.time + 1f / attackRate;
                        }

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
    void FaceTowardsMouse()
    {
        //// Convert mouse to world at the player's depth
        //float screenZ = Camera.main.WorldToScreenPoint(transform.position).z;

        //Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ));
        //targetPos = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

        //bool mouseIsLeft = mouseWorld.x < transform.position.x;
        ////if (spriteRenderer != null)
        ////{
        ////    spriteRenderer.flipX = mouseIsLeft; // left = true, right = false
        ////}
        ////else
        ////{
        ////    // fallback if no SpriteRenderer provided
        ////    Vector3 s = transform.localScale;
        ////    s.x = mouseIsLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
        ////    transform.localScale = s;
        ////}
        //if (targetPos.x < transform.position.x)
        //{
        //    // Face left
        //    transform.localScale = new Vector3(-1, 1, 1);
        //}
        //else if (targetPos.x > transform.position.x)
        //{
        //    // Face right
        //    transform.localScale = new Vector3(1, 1, 1);
        float screenZ = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ));

        spriteRenderer.flipX = (mouseWorld.x < transform.position.x);
        
    }
 }



     

    



