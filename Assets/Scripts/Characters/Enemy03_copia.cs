using UnityEngine;
using System.Collections;

public class Enemy03_copia : BaseCharacter

{
    [System.Serializable]
    public class ZoneBehavior
    {
        public BehaviorZone.BehaviorType behaviorType;

        [Header("Movimiento")]
        public float moveSpeedMultiplier = 1f;
        public float rotationSpeed = 3f;

        [Header("Animación")]
        [AnimationParameter]
        public string animationTrigger;

        [Header("Apariencia")]
        public Material zoneMaterial;
        public float materialTransitionDuration = 1f;
        public Color emissionColor = Color.white;
        public float emissionIntensity = 1f;

        [Header("Efectos")]
        public ParticleSystem characterEffect;
        public AudioClip zoneEnterSound;
        public AudioClip ambientSound;
        public float soundVolume = 1f;
    }

    [Header("Configuración")]
    [SerializeField]
    private ZoneBehavior[] zoneBehaviors = new ZoneBehavior[0];
    public float baseSpeed = 5f;
    public bool showDebugLogs = true;

    [Header("Referencias")]
    private GameObject player;
    private Vector3 startPosition;
    private ParticleSystem currentEffect;
    private Material currentMaterial;
    private Coroutine materialTransitionCoroutine;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();

        // Inicializar Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null && showDebugLogs)
        {
            Debug.LogWarning("No se encontró el Player. Asegúrate de que tiene el tag 'Player'");
        }

        startPosition = transform.position;
        InitializeCharacter();
    }

    private void ConfigureRigidbody()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation |
                           RigidbodyConstraints.FreezePositionY;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public override void OnEnterBehaviorZone(BehaviorZone.BehaviorType zoneType)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Entrando en zona: {zoneType}");
        }

        // Guardar el tipo de zona actual
        currentBehaviorType = zoneType;

        // Actualizar comportamiento
        UpdateBehavior(zoneType);

        base.OnEnterBehaviorZone(zoneType);
    }
    private void InitializeDefaultBehavior()
    {
        // Configurar comportamiento por defecto (Normal)
        var defaultBehavior = new ZoneBehavior
        {
            behaviorType = BehaviorZone.BehaviorType.Normal,
            moveSpeedMultiplier = 1f,
            rotationSpeed = 3f,
            animationTrigger = "Armature|Action_Walk"  // O la animación que quieras por defecto
        };

        // Añadir este comportamiento al inicio del array
        if (zoneBehaviors == null || zoneBehaviors.Length == 0)
        {
            zoneBehaviors = new ZoneBehavior[] { defaultBehavior };
        }
        else
        {
            var newBehaviors = new ZoneBehavior[zoneBehaviors.Length + 1];
            newBehaviors[0] = defaultBehavior;
            zoneBehaviors.CopyTo(newBehaviors, 1);
            zoneBehaviors = newBehaviors;
        }

        // Establecer el comportamiento inicial
        currentBehaviorType = BehaviorZone.BehaviorType.Normal;
    }
    private void InitializeCharacter()
    {
        if (meshRenderer != null)
        {
            currentMaterial = new Material(meshRenderer.material);
            meshRenderer.material = currentMaterial;
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("No se encontró SkinnedMeshRenderer en " + gameObject.name);
        }
    }

    private ZoneBehavior GetCurrentBehavior()
    {
        if (zoneBehaviors == null || zoneBehaviors.Length == 0)
        {
            Debug.LogWarning("No hay comportamientos configurados");
            return null;
        }

        // Buscar el comportamiento específico
        foreach (var behavior in zoneBehaviors)
        {
            if (behavior != null && behavior.behaviorType == currentBehaviorType)
            {
                return behavior;
            }
        }

        // Si no encuentra el comportamiento específico, usar el comportamiento normal
        foreach (var behavior in zoneBehaviors)
        {
            if (behavior != null && behavior.behaviorType == BehaviorZone.BehaviorType.Normal)
            {
                return behavior;
            }
        }

        // Si no encuentra ninguno, usar el primero
        return zoneBehaviors[0];
    }
    public override void OnExitBehaviorZone(BehaviorZone.BehaviorType zoneType)
    {
        // Cuando sale de cualquier zona, volver al comportamiento normal
        currentBehaviorType = BehaviorZone.BehaviorType.Normal;
        UpdateBehavior(BehaviorZone.BehaviorType.Normal);

        // Llamar al método base si es necesario
        base.OnExitBehaviorZone(zoneType);
    }
    protected override void UpdateBehavior(BehaviorZone.BehaviorType zoneType)
    {
        Debug.Log($"Enemy03: UpdateBehavior called with type {zoneType}");

        currentBehaviorType = zoneType;
        var behavior = GetCurrentBehavior();

        if (behavior == null)
        {
            Debug.LogError("No behavior found!");
            return;
        }

        Debug.Log($"Found behavior - Speed: {behavior.moveSpeedMultiplier}, " +
                  $"Rotation: {behavior.rotationSpeed}, " +
                  $"Animation: {behavior.animationTrigger}");

        // Aplicar cambios inmediatamente
        if (behavior.moveSpeedMultiplier <= 0)
        {
            // Detener inmediatamente
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
        }

        // Forzar la animación
        if (animator != null && !string.IsNullOrEmpty(behavior.animationTrigger))
        {
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(param.name);
                }
            }
            animator.SetTrigger(behavior.animationTrigger);
            Debug.Log($"Trigger set: {behavior.animationTrigger}");
        }

        // Actualizar otros aspectos
        UpdateMaterial(behavior);
        UpdateEffects(behavior);
        UpdateSounds(behavior);
    }

    private void UpdateAnimation(ZoneBehavior behavior)
    {
        if (animator != null && !string.IsNullOrEmpty(behavior.animationTrigger))
        {
            // Debug de animación
            if (showDebugLogs)
            {
                Debug.Log($"Cambiando animación a: {behavior.animationTrigger}");
            }

            // Resetear todos los triggers primero
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(param.name);
                }
            }

            // Asegurarse de que el parámetro existe antes de activarlo
            AnimatorControllerParameter[] parameters = animator.parameters;
            bool parameterExists = false;
            foreach (var param in parameters)
            {
                if (param.name == behavior.animationTrigger)
                {
                    parameterExists = true;
                    break;
                }
            }

            if (parameterExists)
            {
                animator.SetTrigger(behavior.animationTrigger);
            }
            else
            {
                Debug.LogWarning($"Animación no encontrada: {behavior.animationTrigger}");
            }
        }
    }

    private void UpdateMaterial(ZoneBehavior behavior)
    {
        if (behavior.zoneMaterial != null && meshRenderer != null)
        {
            if (materialTransitionCoroutine != null)
            {
                StopCoroutine(materialTransitionCoroutine);
            }
            materialTransitionCoroutine = StartCoroutine(TransitionMaterial(behavior));
        }
    }

    private IEnumerator TransitionMaterial(ZoneBehavior behavior)
    {
        if (meshRenderer == null || behavior.zoneMaterial == null) yield break;

        Material startMaterial = new Material(currentMaterial);
        Material targetMaterial = new Material(behavior.zoneMaterial);

        // Configurar propiedades URP
        if (startMaterial.HasProperty("_BaseColor") && targetMaterial.HasProperty("_BaseColor"))
        {
            Color startColor = startMaterial.GetColor("_BaseColor");
            Color targetColor = targetMaterial.GetColor("_BaseColor");

            // Configurar emisión
            if (startMaterial.HasProperty("_EmissionColor") && targetMaterial.HasProperty("_EmissionColor"))
            {
                startMaterial.SetColor("_EmissionColor", behavior.emissionColor * behavior.emissionIntensity);
                targetMaterial.SetColor("_EmissionColor", behavior.emissionColor * behavior.emissionIntensity);
            }

            float elapsedTime = 0f;
            while (elapsedTime < behavior.materialTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / behavior.materialTransitionDuration;

                // Interpolar colores
                Color currentColor = Color.Lerp(startColor, targetColor, t);
                currentMaterial.SetColor("_BaseColor", currentColor);

                // Interpolar emisión
                if (currentMaterial.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = behavior.emissionColor * (behavior.emissionIntensity * t);
                    currentMaterial.SetColor("_EmissionColor", emissionColor);
                }

                yield return null;
            }
        }

        currentMaterial.CopyPropertiesFromMaterial(targetMaterial);
        Destroy(startMaterial);
        Destroy(targetMaterial);
    }

    private void UpdateEffects(ZoneBehavior behavior)
    {
        if (currentEffect != null)
        {
            Destroy(currentEffect.gameObject);
        }

        if (behavior.characterEffect != null)
        {
            currentEffect = Instantiate(behavior.characterEffect, transform.position, Quaternion.identity, transform);
            currentEffect.Play();
        }
    }

    private void UpdateSounds(ZoneBehavior behavior)
    {
        if (audioSource == null) return;

        if (behavior.zoneEnterSound != null)
        {
            audioSource.PlayOneShot(behavior.zoneEnterSound, behavior.soundVolume);
        }

        if (behavior.ambientSound != null)
        {
            audioSource.clip = behavior.ambientSound;
            audioSource.loop = true;
            audioSource.volume = behavior.soundVolume;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (player != null)
        {
            var behavior = GetCurrentBehavior();
            if (behavior != null && behavior.moveSpeedMultiplier > 0)
            {
                UpdateMovement();
            }
            else
            {
                // Si debe estar quieto, simplemente no actualizamos la posición
                StopMovement();
            }
        }
    }
    private void StopMovement()
    {
        // Para un objeto kinematic, simplemente nos aseguramos de que no se actualice su posición
        if (rb != null)
        {
            rb.MovePosition(rb.position); // Mantiene la posición actual
        }
    }

    private void UpdateMovement()
    {
        var behavior = GetCurrentBehavior();
        if (behavior != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.y = 0; // Mantener movimiento horizontal

            if (rb != null && behavior.moveSpeedMultiplier > 0)
            {
                // Calcular nueva posición
                Vector3 targetPosition = rb.position + direction * baseSpeed *
                    behavior.moveSpeedMultiplier * Time.deltaTime;

                // Usar MovePosition para movimiento kinematic
                rb.MovePosition(targetPosition);

                // Rotar hacia el jugador si la rotación está permitida
                if (behavior.rotationSpeed > 0)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation,
                        behavior.rotationSpeed * Time.deltaTime));
                }
            }
        }
    }



    private void OnDrawGizmosSelected()
    {
        var behavior = GetCurrentBehavior();
        if (behavior != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (currentEffect != null)
            Destroy(currentEffect.gameObject);

        if (materialTransitionCoroutine != null)
            StopCoroutine(materialTransitionCoroutine);
    }


}