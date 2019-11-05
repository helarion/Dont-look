using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class DecalObjet : Objet
{
    [SerializeField] DecalProjectorComponent decal;
    [SerializeField] bool startActivated;
    [SerializeField] bool shouldDelete = false;

    public override void Activate()
    {
        decal.enabled = !decal.enabled;
        isActivated = decal.enabled;
        if (shouldDelete) Destroy(gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        decal.enabled = startActivated;
    }
}
