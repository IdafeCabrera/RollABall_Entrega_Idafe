using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 7, -10); // Posición inicial relativa al jugador
    public float smoothSpeed = 5f;          // Velocidad de seguimiento suave
    public float lookUpOffset = 2f;         // Altura adicional para el punto de mira

    private void Start()
    {
        // Si no se asignó el jugador, intentar encontrarlo
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("No se encontró el jugador!");
                return;
            }
        }

        // Posicionar la cámara inmediatamente en la primera frame
        transform.position = player.position + offset;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // Calcular la posición objetivo
        Vector3 desiredPosition = player.position + offset;

        // Suavizar el movimiento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Hacer que la cámara mire al jugador
        transform.LookAt(player.position + Vector3.up * lookUpOffset);
    }

    // Método para visualizar la configuración en el editor
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, player.position + Vector3.up * lookUpOffset);
        Gizmos.DrawWireSphere(player.position + Vector3.up * lookUpOffset, 0.5f);
    }
}