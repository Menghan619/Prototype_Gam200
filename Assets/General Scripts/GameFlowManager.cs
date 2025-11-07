// GameFlowManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] private int targetKills = 10;
    [SerializeField] private TextMeshProUGUI objectiveText;   // assign in Canvas
    [SerializeField] private string proceedMessage = "Objective complete! Proceed to the door →";

    private int currentKills = 0;
    private bool objectiveComplete = false;

    [Header("Screens")]
    [SerializeField] private GameObject deathPanel;   // set inactive by default
    [SerializeField] private GameObject winPanel;     // set inactive by default

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip winClip;

    [Header("Refs")]
    [SerializeField] private PlayerHealth player;     // drag PlayerHealth here, or auto-find

    [Header("Death Flow")]
    [SerializeField] private CanvasGroup deathPanelGroup; // put your panel’s CanvasGroup here
    [SerializeField] private float deathFadeDelay = 0.15f; // small pause after anim finishes
    [SerializeField] private float deathFadeDuration = 0.6f; // fade time (unscaled)
    [SerializeField] private AudioClip deathClip; // optional SFX
    [SerializeField] private string deathStateName = "Death"; // name OR tag of your death state
    AudioManager audioManager;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!player) player = FindObjectOfType<PlayerHealth>();
        if (player) player.OnDeath += HandlePlayerDeath; // From your PlayerHealth event. :contentReference[oaicite:2]{index=2}

        audioManager = GameObject.FindGameObjectWithTag("AudioMan").GetComponent<AudioManager>();
    }

    //void Start()
    //{
    //    UpdateObjectiveUI();
    //    if (deathPanel) deathPanel.SetActive(false);
    //    if (winPanel) winPanel.SetActive(false);
    //}
    void Start()
    {
        UpdateObjectiveUI();

        if (winPanel) winPanel.SetActive(false);

        if (deathPanelGroup)
        {
            // Make sure panel exists and is active so it can fade
            if (deathPanel && !deathPanel.activeSelf) deathPanel.SetActive(true);
            deathPanelGroup.alpha = 0f;
            deathPanelGroup.interactable = false;
            deathPanelGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback to old behaviour if no CanvasGroup assigned
            if (deathPanel) deathPanel.SetActive(false);
        }
    }


    // Called by CountKillOnDeath when an enemy dies
    public void ReportEnemyKilled()
    {
        if (objectiveComplete) return;
        currentKills++;
        if (currentKills >= targetKills)
        {
            objectiveComplete = true;
            if (objectiveText) objectiveText.text = proceedMessage;
            if (sfxSource && winClip) sfxSource.PlayOneShot(winClip);
            // (Optionally: open door, enable waypoint marker, etc.)
        }
        else
        {
            UpdateObjectiveUI();
        }
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText)
            objectiveText.text = $"Defeat enemies: {currentKills}/{targetKills}";
    }

    //private void HandlePlayerDeath()
    //{
    //    // Lose flow
    //    Time.timeScale = 0f;
    //    if (deathPanel) deathPanel.SetActive(true);
    //}
    private void HandlePlayerDeath()
    {
        StartCoroutine(CoPlayerDeathSequence());
    }

    private System.Collections.IEnumerator CoPlayerDeathSequence()
    {
        // 1) Wait for the player's death animation to fully finish
        //    (either by state name or by tag = "Death")
        var anim = player ? player.GetComponentInChildren<Animator>() : null;
        if (anim)
        {
            // Wait until we are in the death state
            yield return new WaitUntil(() => IsInDeathState(anim));

            // Then wait until that state completes (normalizedTime >= 1 and no transitions)
            yield return new WaitUntil(() => IsDeathStateComplete(anim));
        }

        // 2) Short settle delay (feels nicer)
        yield return new WaitForSecondsRealtime(deathFadeDelay);

        // 3) Optional death SFX
        audioManager.StopMusic();
        if (sfxSource && deathClip) sfxSource.PlayOneShot(deathClip);

        // 4) Fade in panel
        if (deathPanelGroup)
        {
            yield return StartCoroutine(FadeCanvasGroupUnscaled(deathPanelGroup, 0f, 1f, deathFadeDuration));
            deathPanelGroup.interactable = true;
            deathPanelGroup.blocksRaycasts = true;
        }
        else if (deathPanel)
        {
            deathPanel.SetActive(true);
        }

        // 5) Finally pause the game
        Time.timeScale = 0f;
    }

    private bool IsInDeathState(Animator anim)
    {
        var st = anim.GetCurrentAnimatorStateInfo(0);
        // Match by name OR by tag "Death"
        return st.IsName(deathStateName) || st.IsTag("Death");
    }

    private bool IsDeathStateComplete(Animator anim)
    {
        var st = anim.GetCurrentAnimatorStateInfo(0);
        bool inDeath = st.IsName(deathStateName) || st.IsTag("Death");
        bool notTransitioning = !anim.IsInTransition(0);
        return inDeath && notTransitioning && st.normalizedTime >= 1f;
    }

    private System.Collections.IEnumerator FadeCanvasGroupUnscaled(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cg.alpha = to;
    }


    // Called by a trigger at the “door” once objectiveComplete == true
    public void WinLevel()
    {
        if (!objectiveComplete) return;
        Time.timeScale = 0f;
        if (winPanel) winPanel.SetActive(true);
    }

    // UI Buttons
    public void BtnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BtnQuitToMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }
}
