using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Push Settings")]
    public float basePushForce = 10f;
    public float upwardLift = 0.5f;
    public float pushCooldown = 0.5f;

    [Header("Shockwave Effect")]
    public GameObject shockwaveParticleSystemPrefab; // Assign your particle system prefab here
    public float shockwaveGroundOffset = 0.1f; // How far above ground to spawn the effect
    public LayerMask groundLayerMask = 1; // What layers count as ground

    private float lastPushTime;
    private bool isInvulnerable = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Don't push if invulnerable
        if (isInvulnerable) return;

        // Check if other object is a player and not self
        if (collision.gameObject.CompareTag("Player") && Time.time > lastPushTime + pushCooldown)
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            PlayerPush otherPush = collision.gameObject.GetComponent<PlayerPush>();

            // Don't push if the other player is invulnerable
            if (otherPush != null && otherPush.IsInvulnerable()) return;

            if (otherRb != null && otherPlayer != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;

                // Scale knockback if this player is giant
                float pushForce = basePushForce;
                if (transform.localScale.x > 4f) pushForce *= 1.5f;

                // Reduce knockback if the other player is giant
                if (otherPlayer.transform.localScale.x > 4f) pushForce *= 0.5f;

                // Apply knockback
                otherPlayer.ApplyFlyingKnockback(pushDirection, pushForce, upwardLift);

                // Create shockwave effect at collision point
                SpawnShockwaveEffect(collision);

                // Camera shake
                CameraShake.Instance?.Shake(.3f);
                lastPushTime = Time.time;
            }
        }
    }

    private void SpawnShockwaveEffect(Collision collision)
    {
        if (shockwaveParticleSystemPrefab == null) return;

        // Get the collision point
        Vector3 collisionPoint = collision.contacts[0].point;

        // Find the ground position below the collision point
        Vector3 groundPosition = FindGroundPosition(collisionPoint);

        // Spawn the particle system at ground level
        GameObject shockwaveEffect = Instantiate(shockwaveParticleSystemPrefab, groundPosition, Quaternion.identity);

        // Optional: Destroy the effect after a certain time (adjust based on your particle system duration)
        Destroy(shockwaveEffect, 3f);
    }

    private Vector3 FindGroundPosition(Vector3 fromPosition)
    {
        // Since ground is a cube at scale 25,1,18, we can find it more efficiently
        RaycastHit hit;
        Vector3 rayStart = fromPosition + Vector3.up * 2f; // Start slightly above collision point

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f, groundLayerMask))
        {
            // Return the ground position with a small offset
            return hit.point + Vector3.up * shockwaveGroundOffset;
        }
        else
        {
            // Fallback: For a flat ground cube, we can assume Y=0.5 (cube's top surface)
            // Adjust this value based on your ground's actual Y position
            return new Vector3(fromPosition.x, 0.5f + shockwaveGroundOffset, fromPosition.z);
        }
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
}