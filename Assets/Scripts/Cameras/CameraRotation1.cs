/*CameraRotation.cs*/
using UnityEngine;

public class CameraRotation1 : MonoBehaviour
{
    public Transform player;       // Referencia al jugador
    public float rotationSpeed = 90f;  // �ngulo de rotaci�n por pulsaci�n
    public float smoothTime = 0.2f;    // Tiempo de suavizado de la transici�n

    private float targetAngle = 0f;    // �ngulo de destino
    private float currentAngle = 0f;   // �ngulo actual
    private float angleVelocity = 0f;  // Velocidad de cambio de �ngulo (para Mathf.SmoothDampAngle)

    void Update()
    {
        // Detecta las teclas de rotaci�n
        if (Input.GetKeyDown(KeyCode.Q)) // Rota a la izquierda
        {
            targetAngle -= rotationSpeed;
        }
        else if (Input.GetKeyDown(KeyCode.E)) // Rota a la derecha
        {
            targetAngle += rotationSpeed;
        }

        // Suaviza la transici�n usando Mathf.SmoothDampAngle
        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, smoothTime);

        // Calcula la nueva posici�n de la c�mara en torno al jugador
        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        transform.position = player.position + rotation * new Vector3(0, 5, -10);
        transform.LookAt(player);
    }
}
