using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("AUDIO Source")]
    [SerializeField] AudioSource SFX;
    [SerializeField] AudioSource FootstepsSFXManager;
    [SerializeField] AudioSource Music;

    [Header("AUDIO CLIP")]
    public AudioClip WindSlash;
    public AudioClip WaterSlash;
    public AudioClip ComboSlash;
    public AudioClip DashSFX;
    public AudioClip HitSFX;
    public AudioClip EnemyScreamSFX;
    public AudioClip[] FootstepsSFX;
    public AudioClip CombatMusic;
    public AudioClip Menumusic;
    public AudioClip FireDemonCharge;
    public AudioClip WaterDemonDamage;
    public AudioClip WaterDemonChargeAttack;
    public AudioClip NeutralSlash;
    public AudioClip PlayerDamage;
    public AudioClip PlayerDeath;
    private void Start()
    {
        
        Scene AC = SceneManager.GetActiveScene();
        if (AC != null) { 
        
           if( AC.name == "Main Menu")
            {
                Music.clip = Menumusic;
                Music.Play();
            }
            else {

                Music.clip = CombatMusic;
                Music.Play();
            }
        }
    }

    public void StopMusic()
    {
        Music.clip = null;
    }
    public void PlaySFX(AudioClip Clip)
    {
        SFX.PlayOneShot(Clip);
    }

    public void PlayWalk() { 
    
        int random = Random.Range(0,FootstepsSFX.Length);
        var clip = FootstepsSFX[random];
        FootstepsSFXManager.PlayOneShot(clip);

    }
}
