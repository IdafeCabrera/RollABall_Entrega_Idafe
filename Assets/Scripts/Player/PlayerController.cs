/*PlayerController.cs*/
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine; // Añadimos el namespace de Cinemachine

public class PlayerController : MonoBehaviour
{

    Rigidbody rb;
    public float velocity;
    float moveX, moveY;
    public float jumpForce;
    public int maxJumps = 0;
    private int countJump = 0;
    private bool IsGrounded;
    public float airControl = 0.5f;

    // Referencia a la cámara virtual de Cinemachine
    public CinemachineVirtualCamera virtualCamera;
    private Transform cameraTransform;

    // Para debug
    public bool showDebugInfo = true;
    private float currentCameraAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Si no se asignó la cámara virtual en el inspector, la buscamos
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        // Obtenemos la transformación de la cámara brain
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            cameraTransform = brain.transform;
        }

        if (showDebugInfo && virtualCamera == null)
        {
            Debug.LogWarning("Virtual Camera no encontrada. Asegúrate de asignarla en el inspector o tenerla en la escena.");
        }
    }

    void FixedUpdate()
    {
        if (cameraTransform == null) return;

        // Obtener el ángulo de la cámara en el eje Y
        currentCameraAngle = cameraTransform.eulerAngles.y;

        // Crear un vector de movimiento basado en el input
        Vector3 inputDirection = new Vector3(moveX, 0, moveY);

        // Convertir el movimiento relativo a la cámara en movimiento mundial
        Vector3 movement = Quaternion.Euler(0, currentCameraAngle, 0) * inputDirection;

        // Normalizar el vector de movimiento para mantener la velocidad constante
        if (movement.magnitude > 0)
        {
            movement = movement.normalized;
        }

        // Aplicar el movimiento
        if (IsGrounded)
        {
            rb.AddForce(movement * velocity);
        }
        else
        {
            rb.AddForce(movement * velocity * airControl);
        }

        // Aplicar fricción
        rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);

        // Debug info
        if (showDebugInfo)
        {
            Debug.Log($"Camera Angle: {currentCameraAngle}");
            Debug.Log($"Input Direction: {inputDirection}");
            Debug.Log($"World Movement: {movement}");
        }
    }

    void OnMove(InputValue moveValue)
    {
        Vector2 move = moveValue.Get<Vector2>();
        moveX = move.x;
        moveY = move.y;
    }

    void OnJump(InputValue jumpValue)
    {
        if (countJump < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            countJump++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            countJump = 0;
            IsGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugInfo && cameraTransform != null)
        {
            // Dibujar la dirección del movimiento
            Gizmos.color = Color.blue;
            Vector3 movement = Quaternion.Euler(0, currentCameraAngle, 0) * new Vector3(moveX, 0, moveY);
            Gizmos.DrawLine(transform.position, transform.position + movement.normalized * 2);

            // Dibujar la dirección de la cámara
            Gizmos.color = Color.red;
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            Gizmos.DrawLine(transform.position, transform.position + cameraForward.normalized * 2);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Points"))
        {
            Points point = other.gameObject.GetComponent<Points>();
            if (point != null)
            {
                point.GetPoints();
            }

        }
    }
}