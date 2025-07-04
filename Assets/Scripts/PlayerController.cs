using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1; // 1 for Player 1, 2 for Player 2

    [Header("PC Input Settings")]
    public string horizontalInput = "Horizontal";
    public string verticalInput = "Vertical";

    [Header("Mobile Controls")]
    public FixedJoystick mobileJoystick; // Assign specific joystick for this player
    public GameObject MobileInput;
    [Header("Movement Settings")]
    public float moveForce = 10f;
    public float maxSpeed = 5f;
    public float friction = 0.98f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Animator animator;
    public bool isMobile;

    [Header("Powerup Settings")]
    public float originalPushForce = 10f;
    public float pushForce = 10f;

    [Header("Knockback Settings")]
    public float knockbackResistance = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        // Setup input strings for PC based on player number
        if (!isMobile)
        {
            SetupPCControls();
            MobileInput.SetActive(false );
        }
    }

    void FixedUpdate()
    {
        // Get input from appropriate source
        Vector3 inputDirection = GetInputDirection();
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
            animator.SetBool("Push", isMoving);
        }
    }

    private void SetupPCControls()
    {
        // Setup input axes based on player number
        if (playerNumber == 1)
        {
            // Player 1 uses WASD
            horizontalInput = "Horizontal"; // A/D keys
            verticalInput = "Vertical";     // W/S keys
        }
        else if (playerNumber == 2)
        {
            // Player 2 uses Arrow Keys
            horizontalInput = "HorizontalP2"; // Left/Right arrows
            verticalInput = "VerticalP2";     // Up/Down arrows
        }
    }

    private Vector3 GetInputDirection()
    {
        if (isMobile && mobileJoystick != null)
        {
            // Get input from mobile joystick
            float h = mobileJoystick.Horizontal;
            float v = mobileJoystick.Vertical;
            return new Vector3(h, 0f, v);
        }
        else
        {
            // Get input from PC keyboard
            float h = Input.GetAxis(horizontalInput);
            float v = Input.GetAxis(verticalInput);
            return new Vector3(h, 0f, v);
        }
    }

    public void ResetPlayerState()
    {
        // Reset scale to normal
        transform.localScale = Vector3.one * 3.5f;

        // Reset powerup values
        pushForce = originalPushForce;
        knockbackResistance = 1f;

        // Stop any ongoing tweens
        transform.DOKill();

        // Reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"{gameObject.name} state reset for respawn");
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

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(launchDir * force, ForceMode.Impulse);
    }
}