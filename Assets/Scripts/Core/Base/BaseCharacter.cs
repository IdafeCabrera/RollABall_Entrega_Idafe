using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour, IZoneAffectable
{
    protected Animator animator;
    protected SkinnedMeshRenderer meshRenderer;
    protected BehaviorZone.BehaviorType currentBehaviorType;
    protected Material originalMaterial;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        currentBehaviorType = BehaviorZone.BehaviorType.Normal;
    }

    public virtual void OnEnterBehaviorZone(BehaviorZone.BehaviorType zoneType)
    {
        currentBehaviorType = zoneType;
        UpdateBehavior(zoneType);
    }

    public virtual void OnExitBehaviorZone(BehaviorZone.BehaviorType zoneType)
    {
        currentBehaviorType = BehaviorZone.BehaviorType.Normal;
        UpdateBehavior(BehaviorZone.BehaviorType.Normal);
    }

    protected abstract void UpdateBehavior(BehaviorZone.BehaviorType zoneType);

    protected virtual void OnDisable()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }
}