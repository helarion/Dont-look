using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeArrival : MonoBehaviour
{
    [SerializeField] GameObject smokePrefab;
    [SerializeField] Transform smokeTransform;

    [SerializeField] float firstShakeDuration;
    [SerializeField] float firstShakeIntensity;

    [SerializeField] GameObject door;
    [SerializeField] GameObject debris;

    [SerializeField] GameObject bipede;
    [SerializeField] Transform bipedeTeleportTransform;

    [SerializeField] List<ActivableLamp> lamps = new List<ActivableLamp>();

    bool startedCoroutine = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

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

    IEnumerator BipedeArrivalCoroutine()
    {
        GameManager.instance.ShakeScreen(firstShakeDuration, firstShakeIntensity);
        door.gameObject.SetActive(false);
        Instantiate(smokePrefab, smokeTransform.position, Quaternion.identity);
        yield return new WaitForSeconds(2.0f);
        lamps[0].Desactivate();
        bipede.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(bipedeTeleportTransform.position);
        yield return new WaitForSeconds(1.0f);
        lamps[1].Desactivate();
        yield return new WaitForSeconds(0.4f);
        lamps[2].Desactivate();
        yield return new WaitForSeconds(0.4f);
        lamps[3].Desactivate();
        yield return new WaitForSeconds(0.4f);
        lamps[4].Desactivate();
        yield return null;
    }
}
