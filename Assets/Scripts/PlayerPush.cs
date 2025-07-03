using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float basePushForce = 10f;
    public float upwardLift = 0.5f;
    public float pushCooldown = 0.5f;

    private float lastPushTime;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if other object is a player and not self
        if (collision.gameObject.CompareTag("Player") && Time.time > lastPushTime + pushCooldown)
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();

            if (otherRb != null && otherPlayer != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;

                // Scale knockback if this player is giant
                float pushForce = basePushForce;
                if (transform.localScale.x > 4f) pushForce *= 1.5f;

                // Reduce knockback if the other player is giant
                if (otherPlayer.transform.localScale.x > 4f) pushForce *= 0.5f;

                otherPlayer.ApplyFlyingKnockback(pushDirection, pushForce, upwardLift);
                CameraShake.Instance?.Shake(.3f);
                lastPushTime = Time.time;
            }
        }
    }
}
