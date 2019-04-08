using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Objet
{
    [SerializeField] private Objet target =null;
    private bool isActivated =false;
    private MeshRenderer model;

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
            Activate();
        }
    }

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        target.Activate();
        model.transform.localEulerAngles = new Vector3(0, 180, 0);
    }
}
