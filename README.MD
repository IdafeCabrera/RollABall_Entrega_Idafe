# 🎮 Roll-a-Ball Adventure Game

## 📝 Descripción
Roll-a-Ball Adventure es un juego 3D desarrollado en Unity donde controlas una bola que debe recolectar anillos mientras esquiva enemigos. El juego combina elementos de plataformas, recolección y evasón, creando una experiencia desafiante y divertida.

Roll-a-Ball Adventure evoluciona el concepto clásico de Roll-a-Ball, incorporando mecánicas avanzadas de juego, sistemas dinámicos de dificultad y efectos cinematográficos. El proyecto destaca por su implementación de zonas de peligro dinámicas, efectos de tensión en la cámara y un sistema de audio adaptativo que mejora la inmersión del jugador.

## 🎯 Objetivo del Juego
El objetivo principal es recolectar 10 anillos distribuidos por el nivel mientras evitas a los enemigos. Al recoger el último anillo, se activará una secuencia de victoria con efectos especiales y sonidos.

## 🎹 Controles

### Movimiento del Jugador
- **W/↑**: Mover hacia adelante
- **S/↓**: Mover hacia atrás
- **A/←**: Mover hacia la izquierda
- **D/→**: Mover hacia la derecha
- **Espacio**: Saltar (disponible doble salto)

### Control de Cámara
- **Q**: Rotar cámara a la izquierda (90 grados)
- **E**: Rotar cámara a la derecha (90 grados)
- **R**: Inclinar cámara hacia arriba
- **F**: Inclinar cámara hacia abajo


## 🎯 Características Principales
## 🕹️ Mecánicas Core

Control físico realista mediante Rigidbody con velocidad angular y lineal personalizadas
Sistema de doble salto con control aéreo variable
Detección de suelo mediante raycasting para precisión en saltos

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


## 🎮 Mecánicas de Juego

### Sistema de Movimiento
- Control preciso de la bola con física realista usando Rigidbody
- Sistema de doble salto implementado
- Control en el aire reducido para mayor desafío
- Fricción ajustada para un movimiento suave

### Sistema de Cámara
- Vista en tercera persona con rotación controlada
- Seguimiento suave del jugador
- Ajuste automático de altura y distancia
- Sistema de rotación por pasos de 90 grados
- Límites verticales para mantener una vista jugable

### Coleccionables
- **Anillos**:
  - Rotan constantemente para mejor visibilidad
  - Emiten partículas al ser recolectados
  - Reproducen efectos de sonido
  - Contador en pantalla (X/10)

### Enemigos

#### Enemigo Estático
- Permanece en una posición fija
- Al contacto:
  - Reproduce efectos de sonido
  - Genera sistema de partículas
  - Reinicia la posición del jugador
  - Mantiene el progreso de anillos recolectados

#### Enemigo Perseguidor
- Se mueve hacia el jugador constantemente
- Velocidad configurable
- Mira siempre hacia el jugador
- Mismos efectos de contacto que el enemigo estático
- Mayor dificultad por su movimiento dinámico

## 🛠️ Características Técnicas

### Sistemas Implementados
1. **GameManager**:
   - Singleton para gestión global
   - Control de puntuación
   - Sistema de victoria
   - Gestión de estados del juego

2. **Sistema de Audio**:
   - Música de fondo en loop
   - Efectos de sonido para coleccionables
   - Sonido especial para último anillo
   - Fade out de música al ganar

3. **Sistema de Partículas**:
   - Efectos visuales para coleccionables
   - Efectos de impacto con enemigos
   - Partículas personalizadas para victoria

4. **UI/UX**:
   - Contador de anillos recolectados
   - Panel de victoria
   - Interfaz minimalista y clara

### Optimizaciones
- Frame rate limitado a 60 FPS
- VSync activado
- Gestión eficiente de físicas
- Destrucción automática de efectos temporales

## 🎨 Assets Utilizados
- Modelos 3D propios para anillos y obstáculos
- Efectos de partículas personalizados
- Sonidos y música libres de derechos
- Texturas y materiales optimizados

## 🔧 Requisitos Técnicos
- Unity 2022.3 o superior
- Input System Package
- TextMeshPro Package
- Cinemachine (para sistema de cámara)

## 🚀 Instalación
1. Clonar el repositorio
2. Abrir con Unity 2022.3 o superior
3. Instalar los paquetes requeridos desde el Package Manager
4. Abrir la escena principal en Assets/Scenes

## 🎵 Créditos de Audio
- Música de fondo: [Nombre del autor/fuente]
- Efectos de sonido: [Nombre del autor/fuente]
- Sonido de victoria: [Nombre del autor/fuente]

## 🤝 Contribuciones
Las contribuciones son bienvenidas. Por favor, abre un issue primero para discutir los cambios que te gustaría hacer.

## 📜 Licencia
Este proyecto está bajo la Licencia [tipo de licencia]. Ver el archivo LICENSE para más detalles.

## 👨‍💻 Autor
[Tu nombre o nickname]

## 🎮 Versión del Juego
Versión 1.0.0
