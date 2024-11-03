/*CameraRotation.cs*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRotation : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Configuración de Rotación")]
    public float horizontalRotationStep = 90f;
    public float verticalRotationStep = 30f;
    public float smoothTime = 0.2f;

    [Header("Ángulos Iniciales")]
    [Tooltip("0 = Norte, 90 = Este, 180 = Sur, 270 = Oeste")]
    public float initialHorizontalAngle = 0f;
    public float initialVerticalAngle = 45f;

    [Header("Límites Verticales")]
    public float minVerticalAngle = 15f;
    public float maxVerticalAngle = 75f;

    [Header("Configuración de Cámara")]
    public float baseCameraDistance = 10f;
    public float heightOffset = 5f;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showWorldDirections = true;

    // Teclas de control
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;
    public KeyCode rotateUpKey = KeyCode.R;
    public KeyCode rotateDownKey = KeyCode.F;

    // Variables privadas para rotación
    private float targetHorizontalAngle;
    private float currentHorizontalAngle;
    private float horizontalAngleVelocity = 0f;
    private float targetVerticalAngle;
    private float currentVerticalAngle;
    private float verticalAngleVelocity = 0f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not set in CameraRotation script!");
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null)
            {
                Debug.LogError("No player found! Camera rotation will not work.");
                enabled = false;
                return;
            }
        }

        // Inicializar con los ángulos especificados
        targetHorizontalAngle = initialHorizontalAngle;
        currentHorizontalAngle = initialHorizontalAngle;
        targetVerticalAngle = initialVerticalAngle;
        currentVerticalAngle = initialVerticalAngle;

        // Aplicar la posición inicial inmediatamente
        UpdateCameraPosition();

        if (showDebugInfo)
        {
            Debug.Log($"Camera initialized at - Horizontal: {currentHorizontalAngle}° (0° = North, 90° = East)");
            Debug.Log($"Player world position: {player.position}");
            Debug.Log($"Camera world position: {transform.position}");
        }
    }

    void Update()
    {
        if (player == null) return;

        bool inputDetected = false;

        // Rotación Horizontal
        if (Input.GetKeyDown(rotateLeftKey))
        {
            targetHorizontalAngle = (targetHorizontalAngle - horizontalRotationStep) % 360f;
            inputDetected = true;
            if (showDebugInfo) Debug.Log($"Q pressed - Target Horizontal: {targetHorizontalAngle}° ({GetDirectionName(targetHorizontalAngle)})");
        }

        // Usar la variable para algo
        if (inputDetected && showDebugInfo)
        {
            Debug.Log("Se detectó input de cámara");
        }

        // Solo actualizar la posición si hubo input
        if (inputDetected ||
            Mathf.Abs(currentHorizontalAngle - targetHorizontalAngle) > 0.01f ||
            Mathf.Abs(currentVerticalAngle - targetVerticalAngle) > 0.01f)
        {
            // Suavizado de rotación y actualización de posición
            currentHorizontalAngle = Mathf.SmoothDampAngle(
                currentHorizontalAngle,
                targetHorizontalAngle,
                ref horizontalAngleVelocity,
                smoothTime
            );

            currentVerticalAngle = Mathf.SmoothDamp(
                currentVerticalAngle,
                targetVerticalAngle,
                ref verticalAngleVelocity,
                smoothTime
            );

            UpdateCameraPosition();
        }


        if (Input.GetKeyDown(rotateRightKey))
        {
            targetHorizontalAngle = (targetHorizontalAngle + horizontalRotationStep) % 360f;
            inputDetected = true;
            if (showDebugInfo) Debug.Log($"E pressed - Target Horizontal: {targetHorizontalAngle}° ({GetDirectionName(targetHorizontalAngle)})");
        }

        // Rotación Vertical
        if (Input.GetKeyDown(rotateUpKey))
        {
            float newAngle = targetVerticalAngle + verticalRotationStep;
            if (newAngle <= maxVerticalAngle)
            {
                targetVerticalAngle = newAngle;
                inputDetected = true;
            }
        }
        if (Input.GetKeyDown(rotateDownKey))
        {
            float newAngle = targetVerticalAngle - verticalRotationStep;
            if (newAngle >= minVerticalAngle)
            {
                targetVerticalAngle = newAngle;
                inputDetected = true;
            }
        }

        // Suavizado de rotación
        currentHorizontalAngle = Mathf.SmoothDampAngle(
            currentHorizontalAngle,
            targetHorizontalAngle,
            ref horizontalAngleVelocity,
            smoothTime
        );

        currentVerticalAngle = Mathf.SmoothDamp(
            currentVerticalAngle,
            targetVerticalAngle,
            ref verticalAngleVelocity,
            smoothTime
        );

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        float horizontalRadians = currentHorizontalAngle * Mathf.Deg2Rad;
        float verticalRadians = currentVerticalAngle * Mathf.Deg2Rad;

        float horizontalDistance = baseCameraDistance * Mathf.Cos(verticalRadians);
        float height = baseCameraDistance * Mathf.Sin(verticalRadians) + heightOffset;

        Vector3 offset = new Vector3(
            horizontalDistance * Mathf.Sin(horizontalRadians),
            height,
            horizontalDistance * Mathf.Cos(horizontalRadians)
        );

        transform.position = player.position + offset;
        transform.LookAt(player.position + Vector3.up * heightOffset * 0.5f);
    }

    private string GetDirectionName(float angle)
    {
        angle = (angle + 360f) % 360f;
        if (angle >= 315f || angle < 45f) return "North";
        if (angle >= 45f && angle < 135f) return "East";
        if (angle >= 135f && angle < 225f) return "South";
        return "West";
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || player == null) return;

        Vector3 playerPos = player.position;
        float arrowLength = baseCameraDistance * 0.5f;

        // Dibujar línea de la cámara al jugador
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, playerPos);

        if (showWorldDirections)
        {
            // Norte (Azul)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerPos, Vector3.forward * arrowLength);
            DrawArrowhead(playerPos + Vector3.forward * arrowLength, Vector3.forward, Color.blue);

            // Este (Rojo)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerPos, Vector3.right * arrowLength);
            DrawArrowhead(playerPos + Vector3.right * arrowLength, Vector3.right, Color.red);

            // Sur (Cyan)
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerPos, -Vector3.forward * arrowLength);
            DrawArrowhead(playerPos + -Vector3.forward * arrowLength, -Vector3.forward, Color.cyan);

            // Oeste (Magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(playerPos, -Vector3.right * arrowLength);
            DrawArrowhead(playerPos + -Vector3.right * arrowLength, -Vector3.right, Color.magenta);

            // Dibujar ángulo actual
            Gizmos.color = Color.green;
            Vector3 currentDir = Quaternion.Euler(0, currentHorizontalAngle, 0) * Vector3.forward;
            Gizmos.DrawRay(playerPos, currentDir * arrowLength);
            DrawArrowhead(playerPos + currentDir * arrowLength, currentDir, Color.green);
        }
    }

    private void DrawArrowhead(Vector3 position, Vector3 direction, Color color)
    {
        float arrowSize = 0.5f;
        Vector3 right = Quaternion.Euler(0, 30, 0) * -direction;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -direction;

        Gizmos.color = color;
        Gizmos.DrawRay(position, right * arrowSize);
        Gizmos.DrawRay(position, left * arrowSize);
    }

#if UNITY_EDITOR
    // Botón en el inspector para alinear la cámara con el norte
    [UnityEditor.CustomEditor(typeof(CameraRotation))]
    public class CameraRotationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CameraRotation cameraRotation = (CameraRotation)target;
            if (GUILayout.Button("Alinear con Norte (0°)"))
            {
                cameraRotation.initialHorizontalAngle = 0f;
                cameraRotation.targetHorizontalAngle = 0f;
                cameraRotation.currentHorizontalAngle = 0f;
                cameraRotation.UpdateCameraPosition();
            }
        }
    }
#endif
}