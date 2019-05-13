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
    }

    private void Update()
    {
        velocity = overrideVelocity;
        animator.SetFloat("Velocity", velocity);
    }
    
    public void Trigger()
    {
        animator.SetBool("IsMoving", true);
        MoveTo(endPos.position);
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        base.IsLit(b);
        if (b)
        {
            Trigger();
        }
    }
}
