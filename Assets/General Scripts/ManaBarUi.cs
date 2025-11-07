using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMana playerMana;   // drag your Player object here
    [SerializeField] private Image fillImage;         // assign ManaFill
    [SerializeField] private TextMeshProUGUI label;   // optional TMP showing numbers

    [Header("Smoothing")]
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothSpeed = 8f;  // higher = snappier

    float targetFill = 1f;

    void Awake()
    {
        if (!playerMana) playerMana = FindObjectOfType<PlayerMana>();
    }

    void OnEnable()
    {
        if (playerMana != null)
            playerMana.OnManaChanged += OnManaChanged;
    }

    void OnDisable()
    {
        if (playerMana != null)
            playerMana.OnManaChanged -= OnManaChanged;
    }

    void Start()
    {
        // Force an initial refresh in case we enabled after PlayerMana Awake fired
        if (playerMana != null)
            OnManaChanged(playerMana.CurrentMana, playerMana.maxMana);
    }

    void Update()
    {
        if (!fillImage) return;
        if (smooth)
        {
            float cur = fillImage.fillAmount;
            float next = Mathf.Lerp(cur, targetFill, Time.unscaledDeltaTime * smoothSpeed);
            // snap when very close to avoid endless lerp
            if (Mathf.Abs(next - targetFill) < 0.001f) next = targetFill;
            fillImage.fillAmount = next;
        }
        else
        {
            fillImage.fillAmount = targetFill;
        }
    }

    private void OnManaChanged(int current, int max)
    {
        targetFill = max > 0 ? (float)current / max : 0f;
        if (label) label.text = $"{current}/{max}";
    }

    // Optional helper if you ever change the PlayerMana at runtime:
    public void Bind(PlayerMana pm)
    {
        if (playerMana != null) playerMana.OnManaChanged -= OnManaChanged;
        playerMana = pm;
        if (playerMana != null) playerMana.OnManaChanged += OnManaChanged;
        if (playerMana != null) OnManaChanged(playerMana.CurrentMana, playerMana.maxMana);
    }
}
