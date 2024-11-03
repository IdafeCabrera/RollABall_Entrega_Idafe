using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float velocity;
    private Vector2 moveInput;
    public float jumpForce;
    public int maxJumps = 1;
    private int jumpCount = 0;
    public float airControl = 0.5f;
    private bool isGrounded;


    float moveX, moveY;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        if (isGrounded)
        {
            rb.AddForce(movement * velocity);
        }
        else
        {
            rb.AddForce(movement * velocity * airControl);
        }
        rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Movimiento usando el Input System
    void OnMove1(InputValue moveValue)
    {
        Vector2 move = moveValue.Get<Vector2>();
        moveX = move.x;
        moveY = move.y;
    }




    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpCount < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpCount++;
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (grounded) jumpCount = 0;
    }
}
