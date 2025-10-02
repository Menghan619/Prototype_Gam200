using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("AUDIO Source")]
    [SerializeField] AudioSource SFX;

    [Header("AUDIO CLIP")]
    public AudioClip WindSlash;
    public AudioClip WaterSlash;
    public AudioClip ComboSlash;
    public AudioClip DashSFX;
    public AudioClip HitSFX;
    public AudioClip EnemyScreamSFX;


    public void PlaySFX(AudioClip Clip)
    {
        SFX.PlayOneShot(Clip);
    }
}
