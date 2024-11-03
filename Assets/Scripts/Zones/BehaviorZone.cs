using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controla el comportamiento de zonas que afectan a los personajes del juego.
/// Esta clase permite crear áreas con diferentes efectos como zonas seguras, trampas,
/// zonas de comportamiento específico para enemigos y aliados, y efectos visuales/sonoros.
/// </summary>
public class BehaviorZone : MonoBehaviour
{
    #region Enumeraciones

    /// <summary>
    /// Define qué tipo de personajes son afectados por la zona
    /// </summary>
    public enum CharacterType
    {
        All,        // Afecta tanto a enemigos como aliados
        Enemies,    // Solo afecta a enemigos
        Allies      // Solo afecta a aliados
    }

    /// <summary>
    /// Define los diferentes tipos de comportamiento disponibles para las zonas
    /// </summary>
    public enum BehaviorType
    {
        // Comportamientos básicos
        Normal,     // Comportamiento base del personaje
        Safe,       // Zona segura donde los enemigos no pueden entrar
        Danger,     // Zona peligrosa con efectos negativos

        // Comportamientos de movimiento
        Slow,       // Zona que ralentiza a los personajes
        Teleport,   // Zona para teletransportar

        // Comportamientos de trampa
        Trap,       // Zona que puede activarse como trampa

        // Comportamientos agresivos (enemigos)
        Agressive,  // Comportamiento agresivo básico
        Frenzy,     // Comportamiento muy agresivo

        // Comportamientos defensivos/ayuda (aliados)
        Protect,    // El aliado protege al jugador
        Heal,       // El aliado cura al jugador
        Follow,     // El aliado sigue al jugador
        Guide,      // El aliado guía al jugador
        Support,    // El aliado proporciona apoyo (buffs)

        // Comportamientos tácticos
        Stealth,    // Movimiento sigiloso
        Patrol,     // Patrulla en un área
        Guard,      // Vigilancia estática
        Idle       // Sin movimiento
    }

    /// <summary>
    /// Define cómo la zona afecta al audio del nivel
    /// </summary>
    public enum AudioBehavior
    {
        None,       // No afecta al audio del nivel
        Lower,      // Baja el volumen del audio del nivel
        Pause,      // Pausa el audio del nivel
        Replace     // Reemplaza con el audio de la zona
    }

    #endregion

    #region Variables Públicas con Atributos

    [Header("Configuración de Zona")]
    [Tooltip("Tipo de comportamiento que adoptarán los personajes en esta zona")]
    public BehaviorType zoneType = BehaviorType.Normal;

    [Tooltip("Qué tipo de personajes se ven afectados por esta zona")]
    public CharacterType affectsType = CharacterType.All;

    [Header("Visualización")]
    [Tooltip("Color de la zona para visualización en el editor")]
    public Color zoneColor = Color.white;

    [Tooltip("Mostrar elementos visuales de debug en el editor")]
    public bool showDebugGizmos = true;

    [Tooltip("Mostrar barrera visual de la zona")]
    public bool showVisualBarrier = true;

    [Header("Efectos de Zona")]
    [Tooltip("Material para la barrera visual")]
    public Material zoneMaterial;

    [Tooltip("Altura de la barrera visual")]
    public float barrierHeight = 3f;

    [Tooltip("Sistema de partículas para efectos visuales")]
    public ParticleSystem zoneEffect;

    [Header("Configuración de Audio")]
    [Tooltip("Cómo afecta esta zona al audio del nivel")]
    public AudioBehavior audioBehavior = AudioBehavior.None;

    [Tooltip("Sonido ambiente de la zona")]
    public AudioClip zoneAmbientSound;

    [Tooltip("Volumen del audio de la zona")]
    [Range(0f, 1f)]
    public float zoneAudioVolume = 1f;

    [Header("Configuración de Barrera")]
    [Tooltip("Activar efecto de repulsión en la zona")]
    public bool useBarrier = true;

    [Tooltip("Fuerza con la que se repelen los personajes")]
    public float pushForce = 10f;

    [Header("Configuración de Trampa")]
    [Tooltip("Si la zona puede ser activada como trampa")]
    public bool isTriggerable = false;

    [Tooltip("Duración del efecto de la trampa")]
    public float trapDuration = 5f;

    [Tooltip("Tiempo de espera entre activaciones")]
    public float trapCooldown = 10f;

    #endregion

    #region Variables Privadas

    // Control de estado
    private bool isTrapped = false;           // Estado actual de la trampa
    private float trapTimer = 0f;             // Temporizador para la trampa
    private bool isEffectActive = false;      // Estado de los efectos

    // Componentes y referencias
    private MeshRenderer barrierRenderer;     // Renderer de la barrera visual
    private AudioSource audioSource;          // Fuente de audio de la zona
    private List<ParticleSystem> activeEffects = new List<ParticleSystem>(); // Efectos activos

    #endregion

    #region Métodos Unity

    /// <summary>
    /// Se llama cuando el objeto se activa por primera vez
    /// </summary>
    private void Start()
    {
        InitializeZone();
    }

    /// <summary>
    /// Se llama cuando el objeto se destruye o desactiva
    /// </summary>
    private void OnDestroy()
    {
        CleanupEffects();
    }

    #endregion

    #region Inicialización y Configuración

    /// <summary>
    /// Inicializa todos los componentes y efectos de la zona
    /// </summary>
    private void InitializeZone()
    {
        // Crear barrera visual si está habilitada
        if (showVisualBarrier)
        {
            CreateBarrier();
        }

        // Inicializar efectos de partículas
        InitializeParticleEffects();

        // Configurar sistema de audio
        InitializeAudio();
    }

    /// <summary>
    /// Crea la barrera visual de la zona
    /// </summary>
    private void CreateBarrier()
    {
        if (zoneMaterial == null) return;

        // Crear objeto para la barrera
        GameObject barrier = new GameObject("ZoneBarrier");
        barrier.transform.SetParent(transform);
        barrier.transform.localPosition = Vector3.zero;

        // Añadir componentes visuales
        var meshFilter = barrier.AddComponent<MeshFilter>();
        barrierRenderer = barrier.AddComponent<MeshRenderer>();
        barrierRenderer.material = zoneMaterial;

        // Configurar escala según el collider
        if (TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            barrier.transform.localScale = new Vector3(
                boxCollider.size.x,
                barrierHeight,
                boxCollider.size.z
            );
        }
    }

    /// <summary>
    /// Inicializa el sistema de partículas de la zona
    /// </summary>
    private void InitializeParticleEffects()
    {
        if (zoneEffect != null)
        {
            var effect = Instantiate(zoneEffect, transform.position, Quaternion.identity, transform);
            activeEffects.Add(effect);
        }
    }

    /// <summary>
    /// Configura el sistema de audio de la zona
    /// </summary>
    private void InitializeAudio()
    {
        if (zoneAmbientSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = zoneAmbientSound;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f; // Audio 3D
            audioSource.volume = zoneAudioVolume;

            // Solo reproducir si no reemplaza el audio del nivel
            if (audioBehavior != AudioBehavior.Replace)
            {
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Limpia y destruye todos los efectos activos
    /// </summary>
    private void CleanupEffects()
    {
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
        activeEffects.Clear();
    }

    #endregion


    #region Manejo de Triggers y Colisiones

    /// <summary>
    /// Se llama cuando un collider entra en la zona
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        ProcessTriggerInteraction(other, true);
    }

    /// <summary>
    /// Se llama cuando un collider sale de la zona
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        ProcessTriggerInteraction(other, false);
    }

    /// <summary>
    /// Se llama continuamente mientras un collider está en la zona
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // Procesar efectos continuos según el tipo de zona
        if (zoneType == BehaviorType.Safe)
        {
            HandleSafeZone(other);
        }
        else if (zoneType == BehaviorType.Trap && isTriggerable && !isTrapped)
        {
            HandleTrapZone(other);
        }
    }

    /// <summary>
    /// Procesa la interacción de entrada/salida de la zona
    /// </summary>
    private void ProcessTriggerInteraction(Collider other, bool isEntering)
    {
        // Si es el jugador, notificar a los personajes cercanos
        if (other.CompareTag("Player"))
        {
            NotifyNearbyCharacters(isEntering);
            HandleZoneAudio(isEntering);
            return;
        }

        // Si es un personaje afectable, notificarle directamente
        var affectable = other.GetComponent<IZoneAffectable>();
        if (affectable != null && ShouldAffectCharacter(other))
        {
            if (isEntering)
                affectable.OnEnterBehaviorZone(zoneType);
            else
                affectable.OnExitBehaviorZone(zoneType);
        }
    }

    #endregion

    #region Comportamientos de Zona

    /// <summary>
    /// Maneja el comportamiento de una zona segura
    /// </summary>
    private void HandleSafeZone(Collider other)
    {
        // Repeler enemigos si tienen el tag correcto
        if (other.CompareTag("Enemy") && useBarrier)
        {
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            other.attachedRigidbody?.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Maneja el comportamiento de una zona trampa
    /// </summary>
    private void HandleTrapZone(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateTrap();
        }
    }

    /// <summary>
    /// Activa los efectos de la trampa
    /// </summary>
    public void ActivateTrap()
    {
        if (isTrapped || trapTimer > 0) return;

        isTrapped = true;
        trapTimer = trapDuration;
        StartCoroutine(TrapEffect());
    }

    /// <summary>
    /// Corrutina que maneja los efectos temporales de la trampa
    /// </summary>
    private IEnumerator TrapEffect()
    {
        // Activar efectos visuales
        if (zoneEffect != null)
        {
            var trapEffect = Instantiate(zoneEffect, transform.position, Quaternion.identity);
            activeEffects.Add(trapEffect);
        }

        yield return new WaitForSeconds(trapDuration);

        isTrapped = false;
        trapTimer = trapCooldown;
        CleanupEffects();
    }

    #endregion

    #region Manejo de Audio

    /// <summary>
    /// Maneja los cambios de audio cuando el jugador entra o sale de la zona
    /// </summary>
    /// <param name="entering">True si el jugador está entrando, false si está saliendo</param>
    private void HandleZoneAudio(bool entering)
    {
        if (AudioManager.Instance == null) return;

        switch (audioBehavior)
        {
            case AudioBehavior.Lower:
                if (entering)
                    AudioManager.Instance.FadeOutLevelMusic();
                else
                    AudioManager.Instance.FadeInLevelMusic();
                break;

            case AudioBehavior.Pause:
                if (entering)
                {
                    AudioManager.Instance.PauseLevelMusic();
                    PlayZoneAudio();
                }
                else
                {
                    StopZoneAudio();
                    AudioManager.Instance.ResumeLevelMusic();
                }
                break;

            case AudioBehavior.Replace:
                if (entering)
                {
                    AudioManager.Instance.PauseLevelMusic();
                    PlayZoneAudio();
                }
                else
                {
                    StopZoneAudio();
                    AudioManager.Instance.ResumeLevelMusic();
                }
                break;
        }
    }

    /// <summary>
    /// Reproduce el audio de la zona
    /// </summary>
    private void PlayZoneAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.volume = zoneAudioVolume;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Detiene el audio de la zona
    /// </summary>
    private void StopZoneAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    #endregion

    #region Notificaciones y Verificaciones

    /// <summary>
    /// Notifica a los personajes cercanos sobre la presencia del jugador en la zona
    /// </summary>
    private void NotifyNearbyCharacters(bool isEntering)
    {
        float detectionRadius = 20f;
        var colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var collider in colliders)
        {
            var affectable = collider.GetComponent<IZoneAffectable>();
            if (affectable != null && ShouldAffectCharacter(collider))
            {
                if (isEntering)
                {
                    affectable.OnEnterBehaviorZone(zoneType);
                    if (showDebugGizmos)
                    {
                        Debug.Log($"Notificando entrada a {collider.name} - Tipo: {zoneType}");
                    }
                }
                else
                {
                    affectable.OnExitBehaviorZone(zoneType);
                    if (showDebugGizmos)
                    {
                        Debug.Log($"Notificando salida a {collider.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Verifica si un personaje debe ser afectado por la zona
    /// </summary>
    private bool ShouldAffectCharacter(Collider collider)
    {
        return affectsType == CharacterType.All ||
               (affectsType == CharacterType.Enemies && collider.CompareTag("Enemy")) ||
               (affectsType == CharacterType.Allies && collider.CompareTag("Ally"));
    }

    #endregion

    #region Gizmos y Debug

    /// <summary>
    /// Dibuja gizmos para visualizar la zona en el editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Establecer color según el tipo de zona
        Gizmos.color = zoneType switch
        {
            BehaviorType.Safe => new Color(0, 1, 0, 0.3f),      // Verde para zona segura
            BehaviorType.Danger => new Color(1, 0, 0, 0.3f),    // Rojo para zona de peligro
            BehaviorType.Trap => new Color(1, 1, 0, 0.3f),      // Amarillo para trampas
            _ => new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f)
        };

        // Dibujar la zona según el tipo de collider
        if (TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);

            // Dibujar radio de detección
            Gizmos.DrawWireSphere(transform.position, 20f);
        }
    }

    #endregion

    #region Editor
#if UNITY_EDITOR

    /// <summary>
    /// Crea una zona básica desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Basic Zone", false, 10)]
    private static void CreateBasicZone()
    {
        CreateZoneWithType("Basic Zone", BehaviorType.Normal, Color.white);
    }

    /// <summary>
    /// Crea una zona agresiva desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Combat Zones/Aggressive Zone", false, 11)]
    private static void CreateAggressiveZone()
    {
        CreateZoneWithType("Aggressive Zone", BehaviorType.Agressive, Color.red);
    }

    /// <summary>
    /// Crea una zona sigilosa desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Combat Zones/Stealth Zone", false, 11)]
    private static void CreateStealthZone()
    {
        CreateZoneWithType("Stealth Zone", BehaviorType.Stealth, Color.blue);
    }

    /// <summary>
    /// Crea una zona de curación desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Support Zones/Heal Zone", false, 12)]
    private static void CreateHealZone()
    {
        CreateZoneWithType("Heal Zone", BehaviorType.Heal, Color.green);
    }

    /// <summary>
    /// Crea una zona de protección desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Support Zones/Protect Zone", false, 12)]
    private static void CreateProtectZone()
    {
        CreateZoneWithType("Protect Zone", BehaviorType.Protect, Color.cyan);
    }

    /// <summary>
    /// Crea una zona de patrulla desde el menú
    /// </summary>
    [MenuItem("GameObject/Behavior Zones/Tactical Zones/Patrol Zone", false, 13)]
    private static void CreatePatrolZone()
    {
        CreateZoneWithType("Patrol Zone", BehaviorType.Patrol, Color.yellow);
    }

    /// <summary>
    /// Método helper para crear zonas con configuración específica
    /// </summary>
    private static void CreateZoneWithType(string name, BehaviorType type, Color color)
    {
        // Crear objeto base
        GameObject zone = new GameObject(name);

        // Añadir y configurar collider
        var collider = zone.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(5f, 3f, 5f);

        // Añadir y configurar BehaviorZone
        var behaviorZone = zone.AddComponent<BehaviorZone>();
        behaviorZone.zoneType = type;
        behaviorZone.zoneColor = color;
        behaviorZone.showVisualBarrier = true;

        // Posicionar en la vista actual
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            zone.transform.position = sceneView.pivot;
        }

        // Establecer padre si hay objeto seleccionado
        if (Selection.activeGameObject != null)
        {
            zone.transform.parent = Selection.activeGameObject.transform;
        }

        // Seleccionar el nuevo objeto
        Selection.activeGameObject = zone;
        Undo.RegisterCreatedObjectUndo(zone, "Create " + name);
    }

    /// <summary>
    /// Editor personalizado para BehaviorZone
    /// </summary>
    [CustomEditor(typeof(BehaviorZone))]
    public class BehaviorZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviorZone zone = (BehaviorZone)target;

            EditorGUI.BeginChangeCheck();

            // Dibujar inspector por defecto
            DrawDefaultInspector();

            // Añadir utilidades adicionales
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utilidades", EditorStyles.boldLabel);

            if (GUILayout.Button("Ajustar a Suelo"))
            {
                SnapToGround(zone);
            }

            if (GUILayout.Button("Duplicar Zona"))
            {
                DuplicateZone(zone);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(zone);
            }
        }

        /// <summary>
        /// Ajusta la zona al suelo más cercano
        /// </summary>
        private void SnapToGround(BehaviorZone zone)
        {
            RaycastHit hit;
            if (Physics.Raycast(zone.transform.position + Vector3.up * 100f, Vector3.down, out hit, 1000f))
            {
                Undo.RecordObject(zone.transform, "Snap To Ground");
                zone.transform.position = hit.point;
            }
        }

        /// <summary>
        /// Duplica la zona seleccionada
        /// </summary>
        private void DuplicateZone(BehaviorZone zone)
        {
            GameObject duplicate = Instantiate(zone.gameObject);
            duplicate.name = zone.gameObject.name + " Copy";
            duplicate.transform.position = zone.transform.position + Vector3.right * 5f;
            Undo.RegisterCreatedObjectUndo(duplicate, "Duplicate Zone");
            Selection.activeGameObject = duplicate;
        }
    }
#endif
    #endregion
}