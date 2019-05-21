using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeArrival : Objet
{
    [SerializeField] GameObject smokePrefab;
    [SerializeField] Transform smokeTransform;

    [SerializeField] float firstShakeDuration;
    [SerializeField] float firstShakeIntensity;
    [SerializeField] float waitBetweenExplosionAndBipede = 2;
    [SerializeField] float waitBetweenBipedeAndLamps = 2;
    [SerializeField] float lampWaitTime = 1;
    [SerializeField] float newSpeed = 3;

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
        bipede.GetAgent().speed = bipede.moveSpeed;
        door.gameObject.SetActive(true);
        doorSas.gameObject.SetActive(true);
        brokenDoor.gameObject.SetActive(false);
        foreach(ActivableLamp l in lamps)
        {
            l.Activate();
        }
    }

    IEnumerator BipedeArrivalCoroutine()
    {
        AkSoundEngine.PostEvent(playDoorExplosion, door);
        yield return new WaitForSeconds(10.2f);
        GameManager.instance.ShakeScreen(firstShakeDuration, firstShakeIntensity);
        door.gameObject.SetActive(false);
        doorSas.gameObject.SetActive(false);
        brokenDoor.gameObject.SetActive(true);
        Instantiate(smokePrefab, smokeTransform.position, Quaternion.identity);
        yield return new WaitForSeconds(waitBetweenExplosionAndBipede);
        bipede.GetAgent().Warp(bipedeTeleportTransform.position);
        bipede.GetAgent().speed = newSpeed;
        yield return new WaitForSeconds(waitBetweenBipedeAndLamps);
        lamps[0].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        lamps[1].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        lamps[2].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        lamps[3].Desactivate();
        yield return new WaitForSeconds(lampWaitTime);
        lamps[4].Desactivate();
        yield return null;
    }
}
