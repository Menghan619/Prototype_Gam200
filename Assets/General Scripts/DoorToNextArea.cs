// DoorToNextArea.cs
using UnityEngine;

public class DoorToNextArea : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        GameFlowManager.Instance?.WinLevel();
    }
}
