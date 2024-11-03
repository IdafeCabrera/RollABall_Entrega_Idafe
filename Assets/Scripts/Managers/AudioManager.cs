using UnityEngine;

/// <summary>
/// Gestiona el audio global del juego
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    var go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    [Header("Configuración de Audio")]
    public AudioSource levelMusic;         // Referencia a la música del nivel
    public float fadeSpeed = 1f;           // Velocidad de transición
    public float lowVolume = 0.3f;         // Volumen reducido

    private float originalVolume;          // Volumen original
    private bool isFading = false;         // Control de transición

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Si no se asignó el levelMusic, intentar encontrarlo
        if (levelMusic == null)
        {
            levelMusic = Camera.main?.GetComponent<AudioSource>();
        }

        if (levelMusic != null)
        {
            originalVolume = levelMusic.volume;
        }
    }

    /// <summary>
    /// Reduce gradualmente el volumen de la música del nivel
    /// </summary>
    public void FadeOutLevelMusic()
    {
        if (levelMusic != null)
        {
            StartCoroutine(FadeAudio(levelMusic.volume, lowVolume));
        }
    }

    /// <summary>
    /// Restaura gradualmente el volumen de la música del nivel
    /// </summary>
    public void FadeInLevelMusic()
    {
        if (levelMusic != null)
        {
            StartCoroutine(FadeAudio(levelMusic.volume, originalVolume));
        }
    }

    /// <summary>
    /// Pausa la música del nivel
    /// </summary>
    public void PauseLevelMusic()
    {
        if (levelMusic != null)
        {
            levelMusic.Pause();
        }
    }

    /// <summary>
    /// Reanuda la música del nivel
    /// </summary>
    public void ResumeLevelMusic()
    {
        if (levelMusic != null)
        {
            levelMusic.UnPause();
        }
    }

    private System.Collections.IEnumerator FadeAudio(float startVolume, float targetVolume)
    {
        if (isFading) yield break;
        isFading = true;

        float elapsedTime = 0;
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            levelMusic.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeSpeed);
            yield return null;
        }

        levelMusic.volume = targetVolume;
        isFading = false;
    }
}