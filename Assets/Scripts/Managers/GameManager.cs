// GameManager.cs
using TMPro;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    private int score = 0;
    public TMP_Text scoreText;
    public GameObject panelWin;
    public int totalPoints = 10;
    public static GameManager instance;

    public AudioClip almostCompleteSound;
    private void Awake()
    {
        score = 0;
        Time.timeScale = 1;
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText();
        // Mejorar la optimizaci�n
        Application.targetFrameRate = 60;

        QualitySettings.vSyncCount = 1;
    }
    // Update is called once per frame
    void Update()
    {
        WinLevel();
    }

    // Modificar m�todo para incluir la l�gica del �ltimo punto
    public void AddPoint(int point)
    {
        score += point;
        UpdateScoreText();

        
        if (score == totalPoints )
        {
            // Reproducir sonido especial
            PlayAlmostCompleteSound();
            // Parar la m�sica
            var audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

    
    }

    // Nuevo m�todo para reproducir el sonido especial
    private void PlayAlmostCompleteSound()
    {
        if (almostCompleteSound != null)
        {
            AudioSource.PlayClipAtPoint(almostCompleteSound, Camera.main.transform.position);
        }
    }

    // Nuevo m�todo para verificar si es el �ltimo punto
    public bool IsLastPoint()
    {
        return score == totalPoints - 1;
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString() + "/" + totalPoints.ToString();
        }
    }


    void WinLevel()
    {

        if (score == totalPoints)
        {
            panelWin.SetActive(true);
            Time.timeScale = 0;
        }
    }
}