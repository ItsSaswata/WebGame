using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Debug.Log($"{gameObject.name} fell!");

            // Notify GameManager instead of destroying immediately
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerFell(gameObject);
            }
            else
            {
                // Fallback if no GameManager
                Destroy(gameObject);
            }
        }
    }
}
