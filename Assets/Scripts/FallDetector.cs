using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
            Debug.Log($"{gameObject.name} fell!");
            // Call a GameManager here if you want to declare the winner
        }
    }
}
