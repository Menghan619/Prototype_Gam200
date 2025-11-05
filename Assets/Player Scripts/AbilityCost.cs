using UnityEngine;

[CreateAssetMenu(fileName = "AbilityCosts", menuName = "Config/AbilityCosts")]
public class AbilityCosts : ScriptableObject
{
    [Header("Basic")]
    public int Basic = 0;     // LMB / Neutral

    [Header("Base Abilities")]
    public int WaterQ = 20;   // Water Slash
    public int WindW = 20;   // Wind Dash Slash
    public int FireE = 20;   // Flame Slash (when added)

    [Header("Fusions")]
    public int SteamBurst_WE = 45; // Fire+Water
    // public int FireCyclone_FW = 45; // future
    // public int TyphoonGuard_QW = 45; // future
}
