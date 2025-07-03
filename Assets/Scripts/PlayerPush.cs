using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : MonoBehaviour
{
    public float pushForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody otherRb = collision.rigidbody;
            if (otherRb != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                otherRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                CameraShake.Instance?.Shake(0.3f);
            }
        }
    }
}
