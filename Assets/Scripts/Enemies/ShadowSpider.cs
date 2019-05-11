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
        //VelocityCount();
        velocity = overrideVelocity;
    }
    
    public void Trigger()
    {
        MoveTo(endPos.position);
        animator.SetBool("IsMoving", true);
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        base.IsLit(b);
        if (b)
        {
            MoveTo(endPos.position);
        }
    }
}
