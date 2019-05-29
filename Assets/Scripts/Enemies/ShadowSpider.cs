using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShadowSpider : Enemy
{
    [SerializeField] Transform endPos;
    [SerializeField] float overrideVelocity=1f;

    private void Start()
    {
        Initialize();
        velocity = 0;
        animator.SetFloat("Velocity", velocity);
    }

    public override void PlayWalk()
    {
        
    }

    private void Update()
    {
        if(isMoving)
        {
            if(agent.remainingDistance<=0)
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void Trigger()
    {
        velocity = overrideVelocity;
        animator.SetFloat("Velocity", velocity);
        animator.SetBool("IsMoving", true);
        isMoving = true;
        MoveTo(endPos.position);
    }
}
