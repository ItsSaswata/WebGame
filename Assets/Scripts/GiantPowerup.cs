using UnityEngine;

public class GiantPowerup : MonoBehaviour
{
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.StartCoroutine(pc.ActivateGiantMode(duration));
                Destroy(gameObject); // Remove powerup
            }
        }
    }
}
