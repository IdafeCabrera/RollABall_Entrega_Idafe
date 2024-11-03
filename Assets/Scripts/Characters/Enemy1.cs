using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    private Vector3 firstPosition;
    private GameObject player;

    public AudioClip enemySound;
    public ParticleSystem enemyParticles;

    public float speed = 2.0f;      // Velocidad de movimiento
    public float range = 3.0f;      // Distancia máxima de movimiento
    private Vector3 startPosition;  // Posición inicial para referencia

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        firstPosition = player.transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position; // Guardamos la posición inicial
    }

    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        MovementEnemy();


    }

    void MovementEnemy()
    {
        // Movemos el enemigo de un lado a otro con Mathf.Sin para crear un movimiento oscilante
        float movement = Mathf.Sin(Time.time * speed) * range;
        transform.position = startPosition + new Vector3(movement, 0, 0); // Cambia el eje si quieres moverlo en Z o Y
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
