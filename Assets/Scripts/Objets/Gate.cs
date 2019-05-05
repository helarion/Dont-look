using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : Objet
{
    [SerializeField] private GameObject door =null;
    [SerializeField] private string doorOpenSound;
    Vector3 startRotation;

    private void Start()
    {
        startRotation = transform.eulerAngles;
    }

    public override void Activate()
    {
        door.transform.eulerAngles = new Vector3(0, 90, 0);
        //AkSoundEngine.PostEvent(doorOpenSound,gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        door.transform.eulerAngles = startRotation;
    }
}
