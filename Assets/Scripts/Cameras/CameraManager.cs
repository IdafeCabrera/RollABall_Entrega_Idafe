/*CameraManager.cs*/
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraSetup
    {
        public string name;
        public CinemachineVirtualCamera virtualCamera;
        public KeyCode switchKey = KeyCode.V;
    }

    [Header("Referencias")]
    public PlayerController playerController;
    public Transform player;  // Referencia al Transform del jugador

    [Header("Configuraciones de C�mara")]
    public CameraSetup[] cameraSetups;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private int currentCameraIndex = 0;

    void Start()
    {
        // Validar y buscar referencias necesarias
        InitializeReferences();

        // Configurar c�maras iniciales
        SetupCameras();
    }

    void InitializeReferences()
    {
        // Buscar PlayerController si no est� asignado
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in scene!");
                return;
            }
        }

        // Buscar el Transform del jugador si no est� asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player not found in scene!");
                return;
            }
        }
    }

    void SetupCameras()
    {
        if (cameraSetups == null || cameraSetups.Length == 0)
        {
            Debug.LogError("No camera setups configured in CameraManager!");
            return;
        }

        // Configurar cada c�mara
        for (int i = 0; i < cameraSetups.Length; i++)
        {
            if (cameraSetups[i].virtualCamera != null)
            {
                // Activar solo la primera c�mara
                cameraSetups[i].virtualCamera.gameObject.SetActive(i == 0);

                // Asignar referencias necesarias a los scripts de c�mara
                SetupCameraReferences(cameraSetups[i].virtualCamera.gameObject);
            }
        }

        // Asignar la c�mara inicial al PlayerController
        if (cameraSetups[0].virtualCamera != null && playerController != null)
        {
            playerController.virtualCamera = cameraSetups[0].virtualCamera;
        }

        if (showDebugInfo)
        {
            Debug.Log($"Camera Manager initialized with {cameraSetups.Length} cameras.");
            Debug.Log($"Active camera: {cameraSetups[0].name}");
        }
    }

    void SetupCameraReferences(GameObject cameraObject)
    {
        // Configurar CameraRotation si existe
        var cameraRotation = cameraObject.GetComponent<CameraRotation>();
        if (cameraRotation != null)
        {
            cameraRotation.player = player;
        }

        // Configurar CameraRotation1 si existe
        var cameraRotation1 = cameraObject.GetComponent<CameraRotation1>();
        if (cameraRotation1 != null)
        {
            cameraRotation1.player = player;
        }

        // Aqu� puedes a�adir m�s scripts de c�mara si los tienes
    }

    void Update()
    {
        // Revisar cada configuraci�n de c�mara
        for (int i = 0; i < cameraSetups.Length; i++)
        {
            if (Input.GetKeyDown(cameraSetups[i].switchKey))
            {
                SwitchToCamera(i);
                break;
            }
        }
    }

    public void SwitchToCamera(int index)
    {
        if (index < 0 || index >= cameraSetups.Length)
        {
            Debug.LogError($"Invalid camera index: {index}");
            return;
        }

        // Desactivar la c�mara actual
        if (cameraSetups[currentCameraIndex].virtualCamera != null)
        {
            cameraSetups[currentCameraIndex].virtualCamera.gameObject.SetActive(false);
        }

        // Activar la nueva c�mara
        currentCameraIndex = index;
        if (cameraSetups[currentCameraIndex].virtualCamera != null)
        {
            var newCamera = cameraSetups[currentCameraIndex].virtualCamera;
            newCamera.gameObject.SetActive(true);

            // Asegurar que las referencias est�n configuradas
            SetupCameraReferences(newCamera.gameObject);

            // Actualizar la referencia en el PlayerController
            if (playerController != null)
            {
                playerController.virtualCamera = newCamera;
                if (showDebugInfo)
                {
                    Debug.Log($"Updated PlayerController camera reference to: {newCamera.name}");
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"Switched to camera: {cameraSetups[currentCameraIndex].name}");
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (cameraSetups != null)
        {
            foreach (var setup in cameraSetups)
            {
                if (string.IsNullOrEmpty(setup.name) && setup.virtualCamera != null)
                {
                    setup.name = setup.virtualCamera.name;
                }
            }
        }
    }
#endif
}