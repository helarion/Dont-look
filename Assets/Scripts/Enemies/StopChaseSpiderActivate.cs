using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopChaseSpiderActivate : SpiderBehavior
{
    [SerializeField] Objet[] listObjetToActivate;

    public override void StopChase()
    {
        if(hasPlayedChase)
        {
            foreach (Objet o in listObjetToActivate)
            {
                o.Activate();
            }
        }
        base.StopChase();
    }

    public override void IsLit(bool b)
    {
        if(isChasing)
        {
            base.IsLit(b);
        }
    }
}
