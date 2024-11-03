using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TensionEffectsManager : MonoBehaviour
{
    public Volume tensionVolume;

    // Referencias a los efectos
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    private WhiteBalance whiteBalance;       // Para ajustar la temperatura del color
    private Bloom bloom;                     // Para efectos de luz
    private FilmGrain filmGrain;             // Para añadir ruido atmosférico

    [Header("Configuración de Ambiente")]
    public float minExposure = -8f;          // Oscurecer la escena
    public float maxGrainIntensity = 0.5f;   // Intensidad del ruido
    public float minTemperature = -50f;      // Hacer la escena más fría
    public float maxBloomIntensity = 0.5f;   // Intensidad del bloom

    [Header("Configuración de Efectos")]
    public float maxVignetteIntensity = 0.8f;        // Aumentado para más dramatismo
    public float maxChromaticIntensity = 1f;
    public float maxSaturationReduction = -70f;       // Aumentado para más dramatismo
    public float maxLensDistortion = -0.7f;          // Aumentado para más dramatismo

    [Header("Configuración de Color")]
    public float maxColorTint = -50f;                // Tinte azulado para efecto de terror
    public float minBrightness = -1.5f;             // Oscurecer la escena
    public float maxContrast = 50f;                 // Aumentar contraste

    [Header("Configuración de Pulso")]
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;             // Aumentado para más dramatismo

    private void Awake()
    {
        // Obtener referencias a los efectos
        if (tensionVolume.profile.TryGet(out Vignette vig)) vignette = vig;
        if (tensionVolume.profile.TryGet(out ChromaticAberration chrom)) chromaticAberration = chrom;
        if (tensionVolume.profile.TryGet(out ColorAdjustments color)) colorAdjustments = color;
        if (tensionVolume.profile.TryGet(out LensDistortion lens)) lensDistortion = lens;
        if (tensionVolume.profile.TryGet(out WhiteBalance white)) whiteBalance = white;
        if (tensionVolume.profile.TryGet(out Bloom blm)) bloom = blm;
        if (tensionVolume.profile.TryGet(out FilmGrain grain)) filmGrain = grain; ;

        // Inicializar efectos
        ResetEffects();
    }

    public void UpdateTensionEffects(float intensity)
    {
        // Asegurar que la intensidad esté entre 0 y 1
        intensity = Mathf.Clamp01(intensity);

        // Añadir efecto de pulso si hay alguna intensidad
        if (intensity > 0)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            intensity = Mathf.Clamp01(intensity + pulse);
        }

        // Actualizar Vignette
        if (vignette != null)
        {
            vignette.intensity.value = intensity * maxVignetteIntensity;
            vignette.smoothness.value = 0.3f + (intensity * 0.4f); // Ajuste dinámico de suavidad
        }

        // Actualizar Chromatic Aberration
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = intensity * maxChromaticIntensity;
        }

        // Color Adjustments
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = intensity * maxSaturationReduction;
            colorAdjustments.postExposure.value = intensity * minExposure;  // Oscurecer la escena
            colorAdjustments.contrast.value = intensity * maxContrast;
            // Color más frío y oscuro para tensión
            colorAdjustments.colorFilter.value = Color.Lerp(
                Color.white,
                new Color(0.5f, 0.5f, 0.7f),
                intensity
            );
        }

        // Lens Distortion
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = intensity * maxLensDistortion;
            lensDistortion.scale.value = 1f - (intensity * 0.3f);
        }
        // White Balance para temperatura
        if (whiteBalance != null)
        {
            whiteBalance.temperature.value = intensity * minTemperature;
        }

        // Bloom
        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(0.5f, maxBloomIntensity, intensity);
            // Tinte azulado para el bloom
            bloom.tint.value = Color.Lerp(
                Color.white,
                new Color(0.5f, 0.5f, 1f),
                intensity
            );
        }
        // Film Grain
        if (filmGrain != null)
        {
            filmGrain.intensity.value = intensity * maxGrainIntensity;
            filmGrain.response.value = 0.8f;
        }

        // Actualizar el peso del volumen para una transición suave
        tensionVolume.weight = intensity;
    }

    public void ResetEffects()
    {
        UpdateTensionEffects(0);
    }

    private void OnDestroy()
    {
        ResetEffects();
    }
}