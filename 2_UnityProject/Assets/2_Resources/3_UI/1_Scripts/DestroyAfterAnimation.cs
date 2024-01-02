using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator animator;

    private IEnumerator Start()
    {
        animator = GetComponent<Animator>();

        yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips[0].length + 0.1f);

        Destroy(gameObject);
    }
}
