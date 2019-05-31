using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeArrival : Objet
{
    [SerializeField] GameObject smokePrefab;
    [SerializeField] Transform smokeTransform;
    [SerializeField] ContiniousLightDetector lightDetector;

    [SerializeField] float waitTime = 0.5f;
    [SerializeField] float firstShakeDuration;
    [SerializeField] float firstShakeIntensity;
    [SerializeField] float waitBetweenExplosionAndBipede = 2;
    [SerializeField] float waitBetweenBipedeAndLamps = 2;
    [SerializeField] float lampWaitTime = 1;
    [SerializeField] float newSpeed = 3;
    [SerializeField] float newAnimSpeed = 2;
    [SerializeField] string lampOffSound1;
    [SerializeField] string lampOffSound2;
    [SerializeField] string lampOffSound3;
    [SerializeField] string lampOffSound4;
    [SerializeField] string lampOffSound5;
    [SerializeField] float newColSize = 20;
    [SerializeField] Vector3 newRotation;

    [SerializeField] SpatialSas doorSas;
    [SerializeField] GameObject door;
    [SerializeField] GameObject brokenDoor;
    [SerializeField] GameObject debris;
    [SerializeField] string playDoorExplosion;

    [SerializeField] BipedeBehavior bipede;
    [SerializeField] Transform bipedeTeleportTransform;

    [SerializeField] List<ActivableLamp> lamps = new List<ActivableLamp>();

    bool startedCoroutine = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            if (!startedCoroutine)
            {
                startedCoroutine = true;
                bipede.isInCorridorChase = true;
                StartCoroutine(BipedeArrivalCoroutine());
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        Transform[] bipedeSpawnZone= bipede.GetSpawnZones();
        bipede.GetAgent().Warp(bipedeSpawnZone[0].position);
        bipede.StopChase();
        startedCoroutine = false;
        door.gameObject.SetActive(true);
        doorSas.gameObject.SetActive(true);
        brokenDoor.gameObject.SetActive(false);
        debris.gameObject.SetActive(false);
        lightDetector.Reset();
        foreach(ActivableLamp l in lamps)
        {
            l.Activate();
        }
    }

    IEnumerator BipedeArrivalCoroutine()
    {
        AkSoundEngine.PostEvent(playDoorExplosion, door);
        doorSas.gameObject.SetActive(false);
        yield return new WaitForSeconds(waitTime);
        GameManager.instance.ShakeScreen(firstShakeDuration, firstShakeIntensity);
        door.gameObject.SetActive(false);
        brokenDoor.gameObject.SetActive(true);
        debris.gameObject.SetActive(true);
        Instantiate(smokePrefab, smokeTransform.position, Quaternion.identity);
        yield return new WaitForSeconds(waitBetweenExplosionAndBipede);
        bipede.GetAgent().Warp(bipedeTeleportTransform.position);
        bipede.transform.eulerAngles = newRotation;
        bipede.moveSpeed = newSpeed;
        bipede.animator.SetFloat("MoveSpeed", newAnimSpeed);
        yield return new WaitForSeconds(waitBetweenBipedeAndLamps);
        AkSoundEngine.PostEvent(lampOffSound1, lamps[0].gameObject);
        lamps[0].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        AkSoundEngine.PostEvent(lampOffSound2, lamps[1].gameObject);
        lamps[1].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        AkSoundEngine.PostEvent(lampOffSound3, lamps[2].gameObject);
        lamps[2].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        AkSoundEngine.PostEvent(lampOffSound4, lamps[3].gameObject);
        lamps[3].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        AkSoundEngine.PostEvent(lampOffSound5, lamps[4].gameObject);
        lamps[4].Desactivate();
        lightDetector.enabled = true;
        lightDetector.Fix();
        yield return null;
    }
}
