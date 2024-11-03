/* Points.cs */
using UnityEngine;

public class Points : MonoBehaviour
{
    [Header("Basic Settings")]
    public AudioClip pointSound;
    public AudioClip lastPointSound;
    public ParticleSystem pointParticles;
    public ParticleSystem collectParticles;

    [Header("Animation Settings")]
    public float rotationSpeed = 100f;
    public Vector3 rotationAxis = Vector3.up;
    public bool useFloatingAnimation = true;
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    [Header("Visual Effects")]
    public bool useGlow = true;
    public Color glowColor = new Color(1f, 0.92f, 0.016f, 1f); // Dorado
    public float glowIntensity = 2f;

    private Vector3 startPosition;
    private float timeOffset;
    private Material ringMaterial;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);

        // Si queremos añadir brillo al material del modelo
        if (useGlow)
        {
            SetupGlowEffect();
        }
    }

    void SetupGlowEffect()
    {
        // Obtener el renderer del modelo
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // Crear una nueva instancia del material para no afectar a otros objetos
            ringMaterial = new Material(renderer.material);
            ringMaterial.EnableKeyword("_EMISSION");
            ringMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
            renderer.material = ringMaterial;
        }
    }

    void Update()
    {
        // Rotación constante
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // Animación de flotación
        if (useFloatingAnimation)
        {
            float yOffset = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;
            transform.position = startPosition + new Vector3(0, yOffset, 0);
        }

        // Actualizar intensidad del brillo si está activado
        if (useGlow && ringMaterial != null)
        {
            float glowVariation = 1f + 0.2f * Mathf.Sin(Time.time * 4f);
            ringMaterial.SetColor("_EmissionColor", glowColor * (glowIntensity * glowVariation));
        }
    }

    public void GetPoints()
    {
        // Determinar qué sonido reproducir al coger el último punto
        AudioClip soundToPlay = GameManager.instance.IsLastPoint() ? lastPointSound : pointSound;


        // Instanciar Sonido
        if (soundToPlay != null)
        {
            GameObject audioObject = new GameObject("PointEffectSound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = soundToPlay;
            audioSource.Play();
            Destroy(audioObject, soundToPlay.length);
        }

        // Instanciar Particulas
        if (pointParticles != null)
        {
            ParticleSystem particles = Instantiate(pointParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        // Efecto de sonido
        if (pointSound != null)
        {
            // Crear un GameObject temporal para reproducir el sonido
            GameObject tempAudioObject = new GameObject("PointSound");
            tempAudioObject.transform.position = transform.position;

            AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();
            audioSource.clip = pointSound;
            //audioSource.spatialBlend = 0.5f; // Mezcla del audio entre 2D y 3D
            audioSource.Play();

            // Destruir el GameObject temporal cuando termine el audio
            Destroy(tempAudioObject, pointSound.length);
        }

        // Efecto de partículas
        if (collectParticles != null)
        {
            ParticleSystem particles = Instantiate(collectParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }

        // Añadir punto y destruir el objeto principal
        GameManager.instance.AddPoint(1);
        Destroy(gameObject); // El objeto se destruye inmediatamente porque el audio está en un GameObject separado
    }

    void OnDestroy()
    {
        if (ringMaterial != null)
        {
            Destroy(ringMaterial);
        }
    }
}
