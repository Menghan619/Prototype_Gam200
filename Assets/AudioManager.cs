using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("AUDIO Source")]
    [SerializeField] AudioSource SFX;
    [SerializeField] AudioSource FootstepsSFXManager;

    [Header("AUDIO CLIP")]
    public AudioClip WindSlash;
    public AudioClip WaterSlash;
    public AudioClip ComboSlash;
    public AudioClip DashSFX;
    public AudioClip HitSFX;
    public AudioClip EnemyScreamSFX;
    public AudioClip[] FootstepsSFX;


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
