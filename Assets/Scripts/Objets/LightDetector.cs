using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetector : Objet
{
    [SerializeField] private Objet target =null;
    [SerializeField] private float delayActivate = 1.5f;
    [SerializeField] private BlinkingLight blinkLight = null;
    [SerializeField] private string activateSound = null;
    private bool isActivated =false;
    private MeshRenderer model;
    private bool isLooked = false;
    private float countLook=0f;

    private void Start()
    {
        model = GetComponentInChildren<MeshRenderer>();
    }

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
                    test = true;
                }
                //print("Touched " + hit.transform.gameObject.name);
            }
        }
        if (!isLooked && !isActivated && test) StartCoroutine("CountLook");
        else if (!test &&!isActivated)
        {
            StopCoroutine("CountLook");
            if(!isActivated && isLooked) blinkLight.StartBlink();
            isLooked = false;
            countLook = 0f;
        }
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'OBJET EST REGARDE PAR LE JOUEUR
    private IEnumerator CountLook()
    {
        isLooked = true;
        blinkLight.StartLook(delayActivate);
        while (countLook < delayActivate)
        {
            yield return new WaitForSeconds(0.1f);
            countLook += 0.1f;
        }
        Activate();
        isLooked = false;
        yield return null;
    }

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        target.Activate();
        isActivated = true;
        print("Object activated");
    }

    public override void Reset()
    {
        base.Reset();
        blinkLight.Reset();
        isActivated = false;
        isLooked = false;
        countLook = 0f;
    }
}
