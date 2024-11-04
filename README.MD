# 🎮 Roll-a-Ball Advanced Adventure Game

## 📝 Descripción
Roll-a-Ball Adventure es un juego 3D desarrollado en Unity donde controlas una bola que debe recolectar anillos mientras esquiva enemigos. El juego combina elementos de plataformas, recolección y evasión, creando una experiencia desafiante y divertida.

Este proyecto evoluciona el concepto clásico de Roll-a-Ball, incorporando mecánicas avanzadas de juego, sistemas dinámicos de dificultad y efectos cinematográficos. Destaca por su implementación de zonas de peligro dinámicas, efectos de tensión en la cámara y un sistema de audio adaptativo que mejora la inmersión del jugador.

## 🎯 Objetivo del Juego
El objetivo principal es recolectar 10 anillos distribuidos por el nivel mientras evitas a los enemigos. Al recoger el último anillo, se activará una secuencia de victoria con efectos especiales y sonidos.

## 🎮 Controles y Gameplay

### Controles de Movimiento
- **W/↑**: Mover hacia adelante
- **S/↓**: Mover hacia atrás
- **A/←**: Mover hacia la izquierda
- **D/→**: Mover hacia la derecha
- **Espacio**: Saltar / Doble salto
- **Shift**: Sprint

### Control de Cámara
- **Q**: Rotar cámara a la izquierda (90 grados)
- **E**: Rotar cámara a la derecha (90 grados)
- **R**: Inclinar cámara hacia arriba
- **F**: Inclinar cámara hacia abajo
- **Mouse Wheel**: Zoom dinámico

## 🎮 Mecánicas de Juego

### Sistema de Movimiento
- Control preciso de la bola con física realista usando Rigidbody.
- Sistema de doble salto implementado, con control aéreo reducido para mayor desafío.
- Detección de suelo mediante raycasting para precisión en saltos.
- Fricción ajustada para un movimiento suave.

```csharp
public class PlayerController : MonoBehaviour
{
    private bool IsGrounded;
    public float airControl = 0.5f;
    
    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveX, 0, moveY);

        // Control diferenciado en tierra y aire
        if (IsGrounded)
        {
            rb.AddForce(movement * velocity);
        }
        else
        {
            rb.AddForce(movement * velocity * airControl);
        }
        
        // Fricción suave
        rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);
    }
}
```

### Sistema de Cámara
- Vista en tercera persona con rotación controlada.
- Seguimiento suave del jugador con offset dinámico y ajuste automático de altura basado en el terreno.
- Sistema de rotación por pasos de 90 grados y límites verticales para mantener una vista jugable.
- Implementación de efectos cinematográficos como shake effect, FOV dinámico y aberración cromática en situaciones de peligro.

```csharp
public class CameraEffects : MonoBehaviour 
{
    public float baseFieldOfView = 60f;
    public float maxFovIncrease = 15f;
    public float chromaticAberrationIntensity = 1f;
    
    private void UpdateTensionEffects(float dangerLevel)
    {
        // FOV dinámico
        Camera.main.fieldOfView = baseFieldOfView + (maxFovIncrease * dangerLevel);
        
        // Intensidad de efectos post-proceso
        chromaticAberration.intensity.value = dangerLevel * chromaticAberrationIntensity;
        
        // Shake modulado
        if(dangerLevel > 0.5f)
        {
            ApplyCameraShake(dangerLevel);
        }
    }
}
```

### Coleccionables
- **Anillos**:
  - Rotan constantemente para mejor visibilidad.
  - Emiten partículas al ser recolectados y reproducen efectos de sonido.
  - Contador en pantalla (X/10).

### Enemigos

#### Enemigo Estático
- Permanece en una posición fija.
- Al contacto:
  - Reproduce efectos de sonido.
  - Genera sistema de partículas.
  - Reinicia la posición del jugador.
  - Mantiene el progreso de anillos recolectados.

#### Enemigo Perseguidor
- IA con pathfinding dinámico y velocidad configurable.
- Velocidad adaptativa según distancia al jugador y mira siempre hacia el jugador.
- Predicción de movimiento del jugador para un mayor desafío.

```csharp
public class Enemy02 : MonoBehaviour
{
    public float speed = 2.0f;
    public float range = 3.0f;
    
    void Update()
    {
        if (player != null)
        {
            // Cálculo de dirección con predicción
            Vector3 direction = (player.transform.position - transform.position).normalized;
            
            // Movimiento suavizado
            transform.position += direction * speed * Time.deltaTime;
            
            // Rotación fluida hacia el jugador
            transform.LookAt(player.transform);
        }
    }
}
```

### 🌋 Zonas de Peligro
Sistema dinámico que modifica el comportamiento del juego:
- Modificación en tiempo real del comportamiento de enemigos.
- Efectos visuales mediante shader personalizado.
- Sistema de partículas reactivo.

```csharp
public class DangerZone : MonoBehaviour
{
    public float dangerIntensity = 1.5f;
    public Color dangerColor = Color.red;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Aumentar agresividad
                enemy.speed *= dangerIntensity;
                // Activar efectos visuales
                enemy.ActivateDangerEffects(dangerColor);
            }
        }
    }
}
```

## 🛠️ Características Técnicas

### Sistemas Implementados
1. **GameManager**: Singleton para gestión global, control de puntuación, sistema de victoria y gestión de estados del juego.
2. **Sistema de Audio Adaptativo**: Música dinámica basada en situaciones del juego, sistema de capas de audio para tensión, transiciones suaves entre estados.

```csharp
public class AudioManager : MonoBehaviour
{
    private AudioSource backgroundMusic;
    private AudioSource tensionLayer;
    
    public void UpdateAudioLayers(float dangerLevel)
    {
        // Ajuste suave del volumen de capas
        backgroundMusic.volume = Mathf.Lerp(1f, 0.3f, dangerLevel);
        tensionLayer.volume = Mathf.Lerp(0f, 1f, dangerLevel);
    }
}
```

3. **Sistema de Partículas**: Efectos visuales para coleccionables, impactos con enemigos y celebración de victoria.
4. **UI/UX**: Contador de anillos recolectados, panel de victoria, interfaz minimalista y clara.

### Optimizaciones
- Uso de Object Pooling para partículas y efectos.
- Sistema de LOD para modelos complejos y culling dinámico de efectos visuales.
- Frame rate limitado a 60 FPS y VSync activado.
- Gestión eficiente de físicas y destrucción automática de efectos temporales.

### Shaders Personalizados
- Efecto de distorsión para zonas de peligro.
- Outline dinámico para enemigos.
- Efectos de post-proceso personalizados.

## 🎨 Assets Utilizados
- Modelos 3D: Blender para anillos y obstáculos.
- Texturas: Substance Painter.
- Efectos de partículas personalizados.
- Audio: FMOD Studio para música adaptativa.
- Efectos Visuales: Unity VFX Graph.

## 🔧 Requisitos Técnicos
- Unity 2022.3 o superior.
- Paquetes requeridos:
  - Universal RP
  - Cinemachine
  - Post Processing
  - Input System
  - TextMeshPro

## 🚀 Instalación
1. Clonar el repositorio.
2. Abrir con Unity 2022.3 o superior.
3. Instalar los paquetes requeridos desde el Package Manager.
4. Abrir la escena principal en `Assets/Scenes`.

## 📜 Licencia
Este proyecto está bajo la Licencia MIT. Ver LICENSE.md para más detalles.

## 🤝 Contribuciones
Las contribuciones son bienvenidas. Por favor, revisa CONTRIBUTING.md para las guías de estilo y proceso.

## 🎮 Versión del Juego
Versión 1.0.0

## 👨‍💻 Autor
[Idafe Cabrera Toledo]

## Tutoriales de ayuda
Curso en udemy
Unity 6 y C# Desarrollo de Videojuegos + Unity Muse IA
Curso de fundamentos de Unity donde abordaremos todas las áreas de desarrollo desde cero iniciando por los fundamentos
Creado por Casta Tutors

## 👨‍💻 Tutor 
Tiburcio Cruz Ravelo

