using UnityEngine;
using System.Collections;  // A�ade esta l�nea
public class BoundaryEnemy : MonoBehaviour
{
    private GameObject player;
    private Vector3 firstPosition;    // Posici�n inicial del jugador
    private bool isAwake = false;
    private bool isPlayingWarningSound = false;
    private GameObject warningAudioObject;

    [Header("Efectos Visuales URP")]
    public TensionEffectsManager tensionEffects;
    public float tensionStartDistance = 35f;    // Distancia donde comienza la tensi�n
    public float maxTensionDistance = 20f;      // Distancia de m�xima tensi�n
    public float tensionBuildUpSpeed = 2f;      // Velocidad a la que aumenta la tensi�n
    public float tensionReleaseSpeed = 1f;      // Velocidad a la que se reduce la tensi�n

    private float currentTensionIntensity = 0f;


    [Header("Efectos de Reaparici�n")]
    public float respawnDelay = 2f;              // Tiempo que tarda en poder moverse
    public ParticleSystem respawnParticles;      // Part�culas de reaparici�n
    public float fadeInDuration = 1f;            // Duraci�n del fade in
    private PlayerController playerController;    // Referencia al controlador del jugador

    [Header("UI Effects")]
    public CanvasGroup fadePanel;        // Panel negro para fade
    public float fadeDuration = 0.5f;    // Duraci�n del fade


    [Header("Posici�n y Movimiento")]
    public Vector3 startPosition = new Vector3(0, 10, 50);  // Posici�n inicial configurable
    public bool useCustomStartPosition = true;              // Toggle para usar posici�n personalizada

    [Header("L�mites y Velocidades")]
    public float boundaryLimit = 25f;      // L�mite para activaci�n
    public float warningLimit = 10f;       // L�mite para sonido de advertencia
    public float chaseSpeed = 15f;         // Velocidad de persecuci�n
    public float returnSpeed = 5f;         // Velocidad de retorno

    [Header("Efectos de Estado")]
    public AudioClip awakeSoundEffect;     // Sonido al despertar
    public ParticleSystem awakenParticles; // Part�culas al despertar
    public AudioClip sleepSoundEffect;     // Sonido al dormir
    public ParticleSystem sleepParticles;  // Part�culas al dormir

    [Header("Efectos de Proximidad")]
    public AudioClip warningSound;         // Sonido de advertencia por proximidad
    [Range(0f, 1f)]
    public float warningSoundVolume = 0.5f;// Volumen del sonido de advertencia

    [Header("Efectos de Captura")]
    public AudioClip impactSound;          // Sonido de impacto al atrapar
    public AudioClip victorySound;         // Sonido de satisfacci�n tras atrapar
    public ParticleSystem captureParticles;// Part�culas de captura

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        firstPosition = player.transform.position;

        if (useCustomStartPosition)
        {
            transform.position = startPosition;
        }
    }

    void Start()
    {
        // Aseguramos que el enemigo empiece en la posici�n correcta
        if (useCustomStartPosition)
        {
            transform.position = startPosition;
        }
    }

    void Update()
    {
        if (player == null || tensionEffects == null) return;

        //float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        //float distanceFromCenter = Vector3.Distance(Vector3.zero, new Vector3(player.transform.position.x, 0, player.transform.position.z));

        float distanceFromCenter = Vector3.Distance(Vector3.zero, new Vector3(player.transform.position.x, 0, player.transform.position.z));
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        
        // Actualizar tensi�n
        UpdateTensionIntensity(distanceFromCenter);








        if (distanceFromCenter > boundaryLimit && !isAwake)
        {
            WakeUp();
        }
        else if (distanceFromCenter <= boundaryLimit && isAwake)
        {
            GoToSleep();
        }

        if (isAwake && distanceToPlayer <= warningLimit)
        {
            PlayWarningSound();
        }
        else
        {
            StopWarningSound();
        }

        if (isAwake)
        {
            ChasePlayer();
        }
        else
        {
            ReturnToSleepPosition();
        }
    }

    void UpdateTensionIntensity(float distanceFromCenter)
    {
        if (tensionEffects == null) return;

        float targetIntensity = 0f;

        // Calcular intensidad objetivo basada en la distancia
        if (distanceFromCenter > maxTensionDistance)
        {
            float distanceRange = tensionStartDistance - maxTensionDistance;
            targetIntensity = Mathf.Clamp01((distanceFromCenter - maxTensionDistance) / distanceRange);
        }

        // Aumentar dr�sticamente la intensidad cuando el enemigo est� despierto
        if (isAwake)
        {
            targetIntensity = Mathf.Lerp(targetIntensity, 1f, 0.5f);
        }

        // Suavizar la transici�n de la intensidad
        float speed = targetIntensity > currentTensionIntensity ? tensionBuildUpSpeed : tensionReleaseSpeed;
        currentTensionIntensity = Mathf.Lerp(currentTensionIntensity, targetIntensity, Time.deltaTime * speed);

        // Aplicar la intensidad a los efectos
        tensionEffects.UpdateTensionEffects(currentTensionIntensity);
    }

    void WakeUp()
    {
        isAwake = true;
        PlayEffects(awakeSoundEffect, awakenParticles);
    }
    private void OnDestroy()
    {
        if (tensionEffects != null)
        {
            tensionEffects.ResetEffects();
        }
        StopWarningSound();
        
}


    void GoToSleep()
    {
        isAwake = false;
        PlayEffects(sleepSoundEffect, sleepParticles);
        StopWarningSound();
        // Resetear la tensi�n al dormir
        currentTensionIntensity = 0f;
        tensionEffects.ResetEffects();
    }

    void PlayWarningSound()
    {
        if (!isPlayingWarningSound && warningSound != null)
        {
            warningAudioObject = new GameObject("WarningSound");
            AudioSource audioSource = warningAudioObject.AddComponent<AudioSource>();
            audioSource.clip = warningSound;
            audioSource.volume = warningSoundVolume;
            audioSource.loop = true;
            audioSource.Play();
            isPlayingWarningSound = true;
        }
    }

    void StopWarningSound()
    {
        if (isPlayingWarningSound && warningAudioObject != null)
        {
            Destroy(warningAudioObject);
            isPlayingWarningSound = false;
        }
    }

    void PlayEffects(AudioClip sound, ParticleSystem particles)
    {
        if (sound != null)
        {
            GameObject audioObject = new GameObject("BoundaryEnemySound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = sound;
            audioSource.Play();
            Destroy(audioObject, sound.length);
        }

        if (particles != null)
        {
            ParticleSystem particleInstance = Instantiate(particles, transform.position, Quaternion.identity);
            particleInstance.Play();
            Destroy(particleInstance.gameObject, particles.main.duration);
        }
    }

    void PlayCaptureSequence()
    {
        if (impactSound != null)
        {
            GameObject audioObject = new GameObject("ImpactSound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = impactSound;
            audioSource.Play();
            Destroy(audioObject, impactSound.length);
        }

        if (victorySound != null)
        {
            GameObject victoryObject = new GameObject("VictorySound");
            AudioSource victorySource = victoryObject.AddComponent<AudioSource>();
            victorySource.clip = victorySound;
            victorySource.PlayDelayed(0.3f);
            Destroy(victoryObject, victorySound.length + 0.3f);
        }

        if (captureParticles != null)
        {
            ParticleSystem particleInstance = Instantiate(captureParticles, transform.position, Quaternion.identity);
            particleInstance.Play();
            Destroy(particleInstance.gameObject, captureParticles.main.duration);
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;
        transform.LookAt(player.transform);
    }

    void ReturnToSleepPosition()
    {
        Vector3 targetPosition = useCustomStartPosition ? startPosition : transform.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetPlayerPosition();
        }
    }

    void ResetPlayerPosition1()
    {
        if (player != null)
        {
            PlayCaptureSequence();
            player.transform.position = firstPosition;
            GoToSleep();
            // Resetear la tensi�n expl�citamente
            currentTensionIntensity = 0f;
            tensionEffects.ResetEffects();
        }
    }

    void ResetPlayerPosition()
    {
        if (player != null)
        {
            PlayCaptureSequence();
            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator RespawnSequence()
    {
        // Fade out
        if (fadePanel != null)
        {
            fadePanel.alpha = 0;
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadePanel.alpha = elapsed / fadeDuration;
                yield return null;
            }
        }

        // Desactivar el control del jugador
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Teletransportar al jugador
        player.transform.position = firstPosition;

        // Reproducir part�culas
        if (respawnParticles != null)
        {
            ParticleSystem particleInstance = Instantiate(respawnParticles, firstPosition, Quaternion.identity);
            particleInstance.Play();
            Destroy(particleInstance.gameObject, respawnParticles.main.duration);
        }

        // Resetear efectos
        currentTensionIntensity = 0f;
        tensionEffects.ResetEffects();
        GoToSleep();

        // Fade in
        if (fadePanel != null)
        {
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadePanel.alpha = 1 - (elapsed / fadeDuration);
                yield return null;
            }
        }

        yield return new WaitForSeconds(respawnDelay);

        // Reactivar control
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }



    private void OnDrawGizmosSelected()
    {
        // Dibuja el l�mite de activaci�n (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, boundaryLimit);

        // Dibuja el l�mite de advertencia (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningLimit);

        // Dibuja la posici�n inicial si est� activada (verde)
        if (useCustomStartPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, 1f);
            Gizmos.DrawLine(transform.position, startPosition);
        }
    }
}