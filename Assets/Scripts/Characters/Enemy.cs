/*Enemy.cs*/
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Vector3 firstPosition;
    private GameObject player;

    public AudioClip enemySound;
    public ParticleSystem enemyParticles;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        firstPosition = player.transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        ResetPosition();
    }
    void ResetPosition()
    {           
        // Instanciar Sonido
        if (enemySound != null)
        {
            GameObject audioObject = new GameObject("EnemyEffectSound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = enemySound;
            audioSource.Play();
            Destroy(audioObject, enemySound.length);
        }

        // Instanciar Particulas
        if (enemyParticles != null)
        {
            ParticleSystem particles = Instantiate(enemyParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration);
        }
        
        if (player != null)
        {
            player.transform.position = firstPosition;
        }
    }
}
