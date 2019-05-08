using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objet : MonoBehaviour
{
    [HideInInspector] public bool isActivated = false;
    //[HideInInspector] 
    public bool isActivating = false;

    public virtual void Activate() {}
    public virtual void Desactivate() {}
    public virtual void Reset()
    {
        isActivated = false;
    }
}
