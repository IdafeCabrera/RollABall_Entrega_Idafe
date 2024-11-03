using UnityEngine;

public class GhostAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation()
    {
        animator.SetTrigger("StartAnimation");  // O usa `SetBool` si configuraste un parámetro Bool
    }
}

