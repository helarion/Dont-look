using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrorTrigger : MonoBehaviour
{
    [SerializeField] ShadowSpider shadow;
    [SerializeField] Light lt;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        lt.enabled = true;
        yield return new WaitForSeconds(1);
        shadow.Trigger();
        yield return new WaitForSeconds(2.5f);
        lt.enabled = false;
    }
}
