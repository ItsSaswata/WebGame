using UnityEngine;
using DG.Tweening;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public string horizontalInput = "Horizontal";
    public string verticalInput = "Vertical";
    public float moveForce = 10f;
    public float maxSpeed = 5f;
    public float friction = 0.98f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Animator animator;

    [Header("Powerup Settings")]
    public float originalPushForce = 10f;
    public float pushForce = 10f;

    [Header("Knockback Settings")]
    public float knockbackResistance = 1f; // 1 = normal, <1 = less knockback


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>(); // Assuming Animator is on child
    }

    void FixedUpdate()
    {
        // Get input
        float h = Input.GetAxis(horizontalInput);
        float v = Input.GetAxis(verticalInput);
        Vector3 inputDirection = new Vector3(h, 0f, v);
        bool isMovingInput = inputDirection.magnitude > 0.1f;

        // Apply force
        if (rb.linearVelocity.magnitude < maxSpeed)
            rb.AddForce(inputDirection.normalized * moveForce, ForceMode.Force);

        // Simulate ice friction
        rb.linearVelocity *= friction;

        // Only rotate if player is giving input
        if (isMovingInput)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDirection);
            Quaternion yOnlyRotation = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, yOnlyRotation, rotationSpeed * Time.deltaTime);
        }



        // Animate
        bool isMoving = inputDirection.magnitude > 0.1f;
        if (animator != null)
        {
            animator.SetBool("Walk", isMoving);
            animator.SetBool("Push", isMoving); // Adjust if you use a separate push key
        }
    }
    public IEnumerator ActivateGiantMode(float duration)
    {
        Vector3 originalScale = Vector3.one * 3.5f;
        Vector3 giantScale = Vector3.one * 5f;

        Sequence growSequence = DOTween.Sequence();
        growSequence.Append(transform.DOScale(Vector3.one * 4.2f, 0.1f).SetEase(Ease.OutBack));
        growSequence.AppendInterval(0.05f);
        growSequence.Append(transform.DOScale(Vector3.one * 4.7f, 0.1f).SetEase(Ease.OutBack));
        growSequence.AppendInterval(0.05f);
        growSequence.Append(transform.DOScale(Vector3.one * 5.1f, 0.08f).SetEase(Ease.OutBack));
        growSequence.Append(transform.DOScale(giantScale, 0.08f).SetEase(Ease.OutBack));

        pushForce = originalPushForce * 2f;
        knockbackResistance = 0.5f;

        yield return new WaitForSeconds(duration);

        transform.DOScale(originalScale, 0.4f).SetEase(Ease.InBack);
        pushForce = originalPushForce;
        knockbackResistance = 1f;
    }
    public void ApplyFlyingKnockback(Vector3 direction, float force, float upwardAmount = 0.5f)
    {
        Vector3 launchDir = (direction.normalized + Vector3.up * upwardAmount).normalized;

        rb.linearVelocity = Vector3.zero; // Reset velocity for consistency
        rb.AddForce(launchDir * force, ForceMode.Impulse);
    }



}
