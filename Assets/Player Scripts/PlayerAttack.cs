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

    [Header("Mana")]
    [SerializeField] private AbilityCosts costs;
    private PlayerMana mana;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    void Awake() 
    { 
        
        if (!mana) mana = GetComponent<PlayerMana>(); 
    
    
    }
    private void Start()
    {
        
        isAttacking = false;
        KeyInputs = new StringBuilder();
        AbilitiesScript = GetComponent<Abilities>();
        elementInd = ElementIndicator.GetComponent<SpriteRenderer>();
    }

    //void Update()
    //{
    //    if (KeyInputs.Length >= 2)
    //    {
    //        KeyInputs.Clear();

    //    }

    //    if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W))
    //    {

    //        switch (KeyInputs.Length)
    //        {

    //            case 0:

    //                //KeyInputs.Append(Input.inputString);
    //                //if (KeyInputs.ToString().ToUpper() == "Q")
    //                //{
    //                //    elementInd.sprite = WaterIcon;
    //                //}
    //                //else if (KeyInputs.ToString().ToUpper() == "W")
    //                //{
    //                //    elementInd.sprite = WindIcon;
    //                //}
    //                //break;
    //                KeyInputs.Append(Input.inputString);
    //                // (Optional) only show element indicator once we confirm we can cast on the second key
    //                if (KeyInputs.ToString().ToUpper() == "Q") { elementInd.sprite = WaterIcon; }
    //                else if (KeyInputs.ToString().ToUpper() == "W") { elementInd.sprite = WindIcon; }
    //                break;

    //            case 1:
    //                KeyInputs.Append(Input.inputString);
    //                string InputFinal = KeyInputs.ToString().ToUpper();
    //                Debug.Log(KeyInputs.ToString().ToUpper());
    //                if (InputFinal == "QQ")
    //                {
    //                    //elementInd.sprite = WaterIcon;
    //                    //if (Time.time >= nextAttackTime)
    //                    //{
    //                    //    FaceTowardsMouse();
    //                    //    AbilitiesScript.AttackAbility("QQ");
    //                    //    CharSlashAnime.SetTrigger("WaterAttack");

    //                    //    nextAttackTime = Time.time + 1f / attackRate;
    //                    //    KeyInputs.Clear();
    //                    //}
    //                    int cost = costs ? costs.WaterQ : 20;
    //                    if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
    //                    {
    //                        elementInd.sprite = WaterIcon;
    //                        FaceTowardsMouse();
    //                        AbilitiesScript.AttackAbility("QQ");         // spend happens inside Abilities
    //                        CharSlashAnime.SetTrigger("WaterAttack");
    //                        nextAttackTime = Time.time + 1f / attackRate;
    //                    }
    //                    KeyInputs.Clear();



    //                }
    //                else if (InputFinal == "WW")
    //                {
    //                    //elementInd.sprite = WindIcon;
    //                    //if (Time.time >= nextAttackTime)
    //                    //{
    //                    //    FaceTowardsMouse();
    //                    //    AbilitiesScript.AttackAbility("WW");
    //                    //    CharSlashAnime.SetTrigger("WindAttack");
    //                    //    nextAttackTime = Time.time + 1f / attackRate;
    //                    //    KeyInputs.Clear();
    //                    //}
    //                    int cost = costs ? costs.WindW : 20;
    //                    if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
    //                    {
    //                        elementInd.sprite = WindIcon;
    //                        FaceTowardsMouse();
    //                        AbilitiesScript.AttackAbility("WW");         // spend happens inside Abilities
    //                        CharSlashAnime.SetTrigger("WindAttack");
    //                        nextAttackTime = Time.time + 1f / attackRate;
    //                    }
    //                    KeyInputs.Clear();

    //                }
    //                else if (InputFinal == "QW" || InputFinal == "WQ")
    //                {
    //                    //elementInd.sprite = FireIcon;
    //                    ////CharSlashAnime.SetTrigger("Aoe");
    //                    //if (Time.time >= nextAttackTime)
    //                    //{
    //                    //    FaceTowardsMouse();
    //                    //    AbilitiesScript.AttackAbility("WE");
    //                    //    KeyInputs.Clear();
    //                    //    nextAttackTime = Time.time + 1f / attackRate;
    //                    //}
    //                    int cost = costs ? costs.SteamBurst_WE : 45;
    //                    if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
    //                    {
    //                        elementInd.sprite = FireIcon;                // Steam Burst visual cue
    //                        FaceTowardsMouse();
    //                        AbilitiesScript.AttackAbility("WE");         // spend happens inside Abilities
    //                        nextAttackTime = Time.time + 1f / attackRate;
    //                    }
    //                    KeyInputs.Clear();

    //                }
    //                else
    //                {
    //                    KeyInputs.Clear();
    //                }

    //                break;



    //        }

    //    }


    // }
    void Update()
    {
        // --- Single-key actions outside the combo buffer ---

        // 1) Dash (D): single press, no mana
        if (Input.GetKeyDown(KeyCode.D))
        {
            AbilitiesScript.DashEvade();
        }

        // 2) Basic attack (LMB): short neutral slash, no mana (only if you have it hooked)
        if (Input.GetMouseButtonDown(0))
        {
            // Optional: face + animator trigger for basic slash if you use one
            FaceTowardsMouse();
            // If you have a basic call, uncomment:
            AbilitiesScript.AttackAbility("LMB"); 
            // CharSlashAnime.SetTrigger("BasicAttack");
        }

        // --- 2-key combo buffer for Q / W / E ---

        // Clear any overflow
        if (KeyInputs.Length >= 2) KeyInputs.Clear();

        // Accept Q/W/E as combo keys
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E))
        {
            switch (KeyInputs.Length)
            {
                case 0:
                    KeyInputs.Append(Input.inputString); // first key
                                                         // Optional element hint on first press
                    string first = KeyInputs.ToString().ToUpper();
                    if (first == "Q") elementInd.sprite = WaterIcon;
                    else if (first == "W") elementInd.sprite = WindIcon;
                    else if (first == "E") elementInd.sprite = FireIcon;
                    break;

                case 1:
                    KeyInputs.Append(Input.inputString); // second key
                    string InputFinal = KeyInputs.ToString().ToUpper();
                    Debug.Log(InputFinal);

                    // --- Base abilities ---
                    if (InputFinal == "QQ")
                    {
                        int cost = costs ? costs.WaterQ : 20;
                        if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
                        {
                            elementInd.sprite = WaterIcon;
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("QQ");     // spend inside Abilities
                            CharSlashAnime.SetTrigger("WaterAttack");
                            nextAttackTime = Time.time + 1f / attackRate;
                        }
                        KeyInputs.Clear();
                    }
                    else if (InputFinal == "WW")
                    {
                        int cost = costs ? costs.WindW : 20;
                        if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
                        {
                            elementInd.sprite = WindIcon;
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("WW");     // spend inside Abilities
                            CharSlashAnime.SetTrigger("WindAttack");
                            nextAttackTime = Time.time + 1f / attackRate;
                        }
                        KeyInputs.Clear();
                    }
                    else if (InputFinal == "EE")
                    {
                        int cost = costs ? costs.FireE : 20;
                        if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
                        {
                            elementInd.sprite = FireIcon;
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("EE");     // spend inside Abilities
                            CharSlashAnime.SetTrigger("WaterAttack"); // placeholder trigger
                            nextAttackTime = Time.time + 1f / attackRate;
                        }
                        KeyInputs.Clear();
                    }

                    // --- Fusion: Steam Burst (QE / EQ) ---
                    else if (InputFinal == "QE" || InputFinal == "EQ")
                    {
                        int cost = costs ? costs.SteamBurst_WE : 45;
                        if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
                        {
                            elementInd.sprite = FireIcon;            // Steam visual cue for now
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("QE");     // DoSteamBurst() inside Abilities
                            CharSlashAnime.SetTrigger("WaterAttack"); // placeholder trigger
                            nextAttackTime = Time.time + 1f / attackRate;
                        }
                        KeyInputs.Clear();
                    }
                    // --- Fusion: FireCyclone (WE / EW) ---
                    else if (InputFinal == "WE" || InputFinal == "EW")
                    {
                        int cost = costs ? costs.FireCyclone_FW : 45;
                        if (Time.time >= nextAttackTime && RequireManaOrDeny(cost))
                        {
                            elementInd.sprite = FireIcon;            // Steam visual cue for now
                            FaceTowardsMouse();
                            AbilitiesScript.AttackAbility("WE");     // DoSteamBurst() inside Abilities
                            nextAttackTime = Time.time + 1f / attackRate;
                        }
                        KeyInputs.Clear();
                    }
                    else
                    {
                        // Unused combo (e.g., QW/WQ for now) → clear
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

    bool RequireManaOrDeny(int cost)
    {
        if (!mana) return true; // if mana missing, allow (for safety)
        if (mana.CanAfford(cost)) return true;

        // TODO: feedback (flash UI, play deny SFX)
        // e.g., ManaBarUI.Instance?.PulseDeny();
        Debug.Log("Not enough mana for this ability.");
        return false;
    }
}



     

    



