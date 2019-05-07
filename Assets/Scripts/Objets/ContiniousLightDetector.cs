using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] private string chargingSound = null;

    private void Update()
    {
        LightDetection();
    }

    public void LightDetection()
    {
        bool test = false;
        Vector3 playerPosition = GameManager.instance.player.transform.position;
        Vector3 lightVec = GameManager.instance.player.GetLookAt() - playerPosition;
        Vector3 playerToSpiderVec = transform.position - GameManager.instance.player.transform.position;

        float playerToSpiderLength = playerToSpiderVec.magnitude;
        Light playerLight = GameManager.instance.player.getLight();
        float lightRange = playerLight.range;
        float lightAngle = playerLight.spotAngle / 2.0f;
        if (playerToSpiderLength <= lightRange)
        {
            float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToSpiderVec) / (lightVec.magnitude * playerToSpiderVec.magnitude)) * Mathf.Rad2Deg;
            if (angleFromLight <= lightAngle)
            {
                lightVec = Vector3.RotateTowards(lightVec, playerToSpiderVec, angleFromLight, Mathf.Infinity);

                RaycastHit hit;
                Ray ray = new Ray(playerPosition, lightVec);
                Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetWallsAndMobsLayer());

                if (hit.transform.gameObject.tag == gameObject.tag)
                {
                    if (GameManager.instance.player.GetConcentration()) test = true;
                }
                //print("Touched " + hit.transform.gameObject.name);
            }
        }
        if (test) target.isActivating = true;
        else target.isActivating = false;
    }

    public override void Reset()
    {
        base.Reset();
    }
}
