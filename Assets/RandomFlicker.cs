using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFlicker : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] int indexMax;
    [SerializeField] bool isEnabled=false;

    private void Start()
    {
        if(isEnabled)
        {
            PlayRandomAnim();
        }
    }

    public void PlayRandomAnim()
    {
        int randomIndex = Random.Range(0, indexMax);
        animator.SetInteger("Index", randomIndex);
        animator.SetTrigger("Flicker");
    }

}
