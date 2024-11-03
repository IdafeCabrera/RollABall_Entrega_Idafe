/*CameraRotation.cs*/
using UnityEngine;

public class CameraRotation1 : MonoBehaviour
{
    public Transform player;       // Referencia al jugador
    public float rotationSpeed = 90f;  // Ángulo de rotación por pulsación
    public float smoothTime = 0.2f;    // Tiempo de suavizado de la transición

    private float targetAngle = 0f;    // Ángulo de destino
    private float currentAngle = 0f;   // Ángulo actual
    private float angleVelocity = 0f;  // Velocidad de cambio de ángulo (para Mathf.SmoothDampAngle)

    void Update()
    {
        // Detecta las teclas de rotación
        if (Input.GetKeyDown(KeyCode.Q)) // Rota a la izquierda
        {
            targetAngle -= rotationSpeed;
        }
        else if (Input.GetKeyDown(KeyCode.E)) // Rota a la derecha
        {
            targetAngle += rotationSpeed;
        }

        // Suaviza la transición usando Mathf.SmoothDampAngle
        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, smoothTime);

        // Calcula la nueva posición de la cámara en torno al jugador
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        transform.position = player.position + rotation * new Vector3(0, 5, -10);
        transform.LookAt(player);
    }
}
