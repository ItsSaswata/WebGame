using UnityEngine;

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
}
