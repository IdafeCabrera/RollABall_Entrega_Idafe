using UnityEngine;
using System.Collections;

public class TheApparitionEnemy : MonoBehaviour
{
    private GameObject player;              // Referencia al objeto jugador
    private Vector3 firstPosition;          // Posición inicial del jugador
    private bool isVisible = false;         // Estado de visibilidad del enemigo
    private bool isHunting = false;         // Indica si el enemigo está cazando al jugador
    private Vector3 apparitionPosition;     // Posición en la que el enemigo aparece

    [Header("Efectos de Desvanecimiento")]
    public float fadeOutDuration = 0.5f;    // Duración del efecto de desvanecimiento
    public Material ghostMaterial;          // Material con transparencia aplicado al enemigo
    private Material currentMaterial;       // Material instanciado usado para efectos

    [Header("Límites y Comportamiento")]
    public float boundaryLimit = 25f;       // Distancia desde el centro para activar la aparición
    public float appearDistance = 15f;      // Distancia a la que el enemigo aparece frente al jugador
    public float escapeDistance = 30f;      // Distancia a la que el jugador puede escapar
    public float huntSpeed = 20f;           // Velocidad a la que el enemigo persigue al jugador 


    [Header("Efectos Visuales")]
    public TensionEffectsManager tensionEffects;    // Gestor de efectos de tensión
    public float tensionBuildUpSpeed = 3f;          // Velocidad de aumento de la tensión    
    public float tensionReleaseSpeed = 1f;          // Velocidad de liberación de la tensión
    private float currentTensionIntensity = 0f;     // Intensidad actual de la tensión

    [Header("Efectos sonoros y de particulas en el enemigo")]
    public AudioClip appearSound;
    public AudioClip disappearSound;
    [Range(0f, 1f)]                       // Esto crea un slider en el inspector entre 0 y 1
    public float appearSoundVolume = 0.7f; // Valor por defecto 0.7
    public ParticleSystem appearParticles;
    public ParticleSystem disappearParticles;

    private MeshRenderer meshRenderer;

         // Velocidad de persecución

    private Vector3 initialPosition;
    private bool hasAppeared = false; // Nueva variable para controlar la primera aparición
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        firstPosition = player.transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        initialPosition = transform.position;   // Guardar posición inicial
        SetVisibility(false); // Asegurarnos de que empiece invisible

        // Configurar material
        if (meshRenderer != null && ghostMaterial != null)
        {
            currentMaterial = new Material(ghostMaterial);
            meshRenderer.material = currentMaterial;
        }
    }
    void Start()
    {
        // Asegurarnos de que empiece en su posición inicial e invisible
        transform.position = initialPosition;
        SetVisibility(false);
        isHunting = false;
        hasAppeared = false;

        // Añadir un debug para la advertencia
        Debug.Log("hasAppeared inicializado: " + hasAppeared);
    }
    void Update()
    {
        if (player == null || tensionEffects == null) return;

        float distanceFromCenter = Vector3.Distance(Vector3.zero,
            new Vector3(player.transform.position.x, 0, player.transform.position.z));
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);  // Añadida esta línea
                                                                                                   // Solo aparecer si no ha aparecido antes o si ya desapareció completamente
        if (distanceFromCenter > boundaryLimit && !isHunting && !isVisible)
        {
            StartCoroutine(AppearAndHunt());
            hasAppeared = true;
        }
        // Verificar si el jugador ha escapado
        else if (isHunting && distanceToPlayer > escapeDistance)
        {
            StartCoroutine(DisappearAndReset("¡El jugador ha escapado!"));
        }

        // Verificar si el jugador volvió a la zona segura
        else if (distanceFromCenter <= boundaryLimit && isHunting)
        {
            StartCoroutine(DisappearAndReset("El jugador ha vuelto a la zona segura"));
        }


        // Similar al BoundaryEnemy, pero con aparición
        if (distanceFromCenter > boundaryLimit && !isHunting)
        {
            StartCoroutine(AppearAndHunt());
        }
        else if (distanceFromCenter <= boundaryLimit && isHunting)
        {
            StartCoroutine(DisappearSequence());
        }


        // Si está cazando, perseguir al jugador
        if (isHunting)
        {
            HuntPlayer();
        }

        // Actualizar efectos de tensión
        UpdateTensionEffects(distanceFromCenter);
    }

    IEnumerator AppearAndHunt()
    {
        if (!isVisible && !isHunting)
        {
            // Calcular posición de aparición frente al jugador
            Vector3 directionToPlayer = (player.transform.position - Vector3.zero).normalized;
            apparitionPosition = player.transform.position + directionToPlayer * appearDistance;
            apparitionPosition.y = transform.position.y; // Mantener la altura original

            // Efectos de aparición
            transform.position = apparitionPosition;
            PlaySound(appearSound, appearSoundVolume);
            SpawnParticles(appearParticles);

            yield return new WaitForSeconds(0.5f); // Breve pausa para el efecto

            SetVisibility(true);
            isHunting = true;

            // Hacer que mire al jugador
            transform.LookAt(player.transform);
        }
    }

    IEnumerator DisappearSequence()
    {
        if (isHunting)
        {
            isHunting = false;
            PlaySound(disappearSound);
            SpawnParticles(disappearParticles);

            yield return new WaitForSeconds(0.5f);

            SetVisibility(false);
            currentTensionIntensity = 0f;
            tensionEffects.ResetEffects();
        }
    }

    IEnumerator DisappearAndReset(string reason = "")
    {
        if (isHunting || isVisible)
        {
            isHunting = false;

            // Iniciar partículas de desaparición
            if (disappearParticles != null)
            {
                ParticleSystem particles = Instantiate(disappearParticles, transform.position, Quaternion.identity);
                particles.Play();
            }

            // Reproducir sonido de desaparición
            PlaySound(disappearSound);

            // Efecto de desvanecimiento con distorsión
            if (currentMaterial != null)
            {
                float elapsedTime = 0;
                Vector3 startScale = transform.localScale;
                Vector3 endScale = startScale * 1.2f; // Ligero aumento de escala
                Color startColor = currentMaterial.color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

                while (elapsedTime < fadeOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float normalizedTime = elapsedTime / fadeOutDuration;

                    // Interpolar color (transparencia)
                    currentMaterial.color = Color.Lerp(startColor, endColor, normalizedTime);

                    // Escala suave
                    transform.localScale = Vector3.Lerp(startScale, endScale, normalizedTime);

                    // Opcional: Rotación suave
                    transform.Rotate(Vector3.up * Time.deltaTime * 180f);

                    yield return null;
                }

                // Restaurar escala y rotación
                transform.localScale = startScale;
                transform.rotation = Quaternion.identity;
            }

            SetVisibility(false);
            transform.position = initialPosition;
            currentTensionIntensity = 0f;
            tensionEffects.ResetEffects();

            // Restaurar la opacidad del material
            if (currentMaterial != null)
            {
                Color color = currentMaterial.color;
                currentMaterial.color = new Color(color.r, color.g, color.b, 1);
            }
        }
    }

    void HuntPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.position += direction * huntSpeed * Time.deltaTime;
        transform.LookAt(player.transform);
    }

    void UpdateTensionEffects(float distanceFromCenter)
    {
        float targetTension = 0f;

        if (isHunting)
        {
            targetTension = 0.8f; // Alta tensión durante la persecución
        }
        else if (distanceFromCenter > boundaryLimit)
        {
            float distanceRange = boundaryLimit * 1.5f - boundaryLimit;
            targetTension = Mathf.Clamp01((distanceFromCenter - boundaryLimit) / distanceRange);
        }

        float speed = targetTension > currentTensionIntensity ? tensionBuildUpSpeed : tensionReleaseSpeed;
        currentTensionIntensity = Mathf.Lerp(currentTensionIntensity, targetTension, Time.deltaTime * speed);

        tensionEffects.UpdateTensionEffects(currentTensionIntensity);
    }



    void PlaySound(AudioClip clip, float volume = 1f)  // Añadimos el parámetro volume con valor por defecto
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }

    void SpawnParticles(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            ParticleSystem instance = Instantiate(particleSystem, transform.position, Quaternion.identity);
            instance.Play();
            Destroy(instance.gameObject, instance.main.duration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(CaptureSequence());
        }
    }

    IEnumerator CaptureSequence()
    {
        isHunting = false;
        isVisible = false;

        PlaySound(disappearSound);
        SpawnParticles(disappearParticles);

        if (player != null)
        {
            player.transform.position = firstPosition;
        }

        currentTensionIntensity = 0f;
        tensionEffects.ResetEffects();

        yield return new WaitForSeconds(0.5f);

        SetVisibility(false);
        transform.position = initialPosition; // Asegurarnos de volver a la posición inicial
    }


    void SetVisibility(bool visible)
    {
        isVisible = visible;
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }
    }
}