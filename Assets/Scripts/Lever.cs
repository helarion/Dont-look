using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Objet
{
    [SerializeField] Objet target=null;
    //bool isActivated=false;
    MeshRenderer model;

    private void Start()
    {
        model = GetComponentInChildren<MeshRenderer>();
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        if(GameManager.instance.controls.GetButtonDown("Interact"))
        {

        }
    }

    public override void Activate()
    {
        base.Activate();
        target.Activate();
        model.transform.eulerAngles = new Vector3(0, 180, 0);
    }
}
