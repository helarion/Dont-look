using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShadowSpider : Enemy
{
    [SerializeField] Transform endPos;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        //VelocityCount();
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
