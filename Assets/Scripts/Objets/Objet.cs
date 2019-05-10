using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objet : MonoBehaviour
{
    [SerializeField] public bool isActivated = false;
    [SerializeField] public bool isBroken = false;
    public bool isActivating = false;

    public virtual void Activate() {}
    public virtual void Desactivate() {}
    public virtual void Break()
    {
        this.enabled = false;
    }
    public virtual void Fix() {}
    public virtual void Reset()
    {
        isActivated = false;
    }
}
