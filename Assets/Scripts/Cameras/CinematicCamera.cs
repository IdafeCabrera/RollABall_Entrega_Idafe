using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Configuración de Seguimiento")]
    [Tooltip("Distancia a partir de la cual la cámara comenzará a seguir al jugador")]
    public float followThreshold = 5f;
    public float followSpeed = 5f;

    [Header("Configuración de Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    [Tooltip("Distancia inicial de zoom")]
    public float currentZoom = 10f;

    [Header("Configuración de Altura")]
    [Tooltip("Altura base de la cámara sobre el objetivo")]
    public float baseHeight = 5f;
    [Tooltip("Ángulo de inclinación de la cámara (0 = horizontal, 90 = vertical)")]
    [Range(0, 90)]
    public float tiltAngle = 30f;
    [Tooltip("Velocidad de ajuste de altura")]
    public float heightAdjustSpeed = 2f;
    [Tooltip("Límite inferior de altura")]
    public float minHeight = 2f;
    [Tooltip("Límite superior de altura")]
    public float maxHeight = 15f;

    [Header("Configuración de Rotación")]
    public float rotationSpeed = 90f;
    public float smoothRotationTime = 0.2f;

    [Header("Controles")]
    public KeyCode zoomInKey = KeyCode.T;
    public KeyCode zoomOutKey = KeyCode.Y;
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;
    public KeyCode heightUpKey = KeyCode.R;    // Nueva tecla para subir la cámara
    public KeyCode heightDownKey = KeyCode.F;  // Nueva tecla para bajar la cámara

    // Variables privadas
    private Vector3 targetPosition;
    private float currentRotationAngle;
    private float targetRotationAngle;
    private float rotationVelocity;
    private float currentHeight;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not set in CinematicCamera script!");
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null)
            {
                Debug.LogError("No player found! Camera will not work properly.");
                enabled = false;
                return;
            }
        }

        // Inicializar posición objetivo con la posición actual de la cámara
        targetPosition = transform.position;
        currentRotationAngle = transform.eulerAngles.y;
        targetRotationAngle = currentRotationAngle;
        currentHeight = baseHeight;
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandleRotation();
        HandleHeight();
        HandleFollowing();
        UpdateCameraPosition();
    }

    private void HandleZoom()
    {
        // Controlar zoom con las teclas T e Y
        if (Input.GetKey(zoomInKey))
        {
            currentZoom = Mathf.Max(currentZoom - zoomSpeed * Time.deltaTime, minZoom);
        }
        if (Input.GetKey(zoomOutKey))
        {
            currentZoom = Mathf.Min(currentZoom + zoomSpeed * Time.deltaTime, maxZoom);
        }
    }

    private void HandleHeight()
    {
        // Controlar altura con las teclas R y F
        if (Input.GetKey(heightUpKey))
        {
            currentHeight = Mathf.Min(currentHeight + heightAdjustSpeed * Time.deltaTime, maxHeight);
        }
        if (Input.GetKey(heightDownKey))
        {
            currentHeight = Mathf.Max(currentHeight - heightAdjustSpeed * Time.deltaTime, minHeight);
        }
    }

    private void HandleRotation()
    {
        // Controlar rotación con Q y E
        if (Input.GetKeyDown(rotateLeftKey))
        {
            targetRotationAngle -= rotationSpeed;
        }
        if (Input.GetKeyDown(rotateRightKey))
        {
            targetRotationAngle += rotationSpeed;
        }

        // Suavizar la rotación
        currentRotationAngle = Mathf.SmoothDampAngle(
            currentRotationAngle,
            targetRotationAngle,
            ref rotationVelocity,
            smoothRotationTime
        );
    }

    private void HandleFollowing()
    {
        // Verificar si el jugador está fuera del umbral de seguimiento
        float distanceToPlayer = Vector3.Distance(
            new Vector3(targetPosition.x, 0, targetPosition.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (distanceToPlayer > followThreshold)
        {
            // Calcular nueva posición objetivo
            Vector3 directionToPlayer = (player.position - targetPosition).normalized;
            Vector3 newTargetPosition = player.position - directionToPlayer * followThreshold;
            newTargetPosition.y = player.position.y; // Usar la altura del jugador como referencia

            // Suavizar el movimiento hacia la nueva posición
            targetPosition = Vector3.Lerp(targetPosition, newTargetPosition, followSpeed * Time.deltaTime);
        }
    }

    private void UpdateCameraPosition()
    {
        // Calcular la posición de la cámara basada en el zoom, altura y rotación
        Quaternion rotation = Quaternion.Euler(tiltAngle, currentRotationAngle, 0f);

        // Calcular el offset considerando el zoom y la altura actual
        float horizontalDistance = currentZoom * Mathf.Cos(tiltAngle * Mathf.Deg2Rad);
        float verticalDistance = currentHeight;
        Vector3 targetOffset = rotation * new Vector3(0f, verticalDistance, -horizontalDistance);

        // Actualizar posición y rotación de la cámara
        transform.position = targetPosition + targetOffset;
        transform.LookAt(targetPosition);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || player == null) return;

        // Dibujar el umbral de seguimiento
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, followThreshold);

        // Dibujar línea de altura
        Gizmos.color = Color.green;
        Gizmos.DrawLine(targetPosition, targetPosition + Vector3.up * currentHeight);
    }
}