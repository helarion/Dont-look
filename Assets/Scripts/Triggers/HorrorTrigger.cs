using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrorTrigger : MonoBehaviour
{
    [SerializeField] ShadowSpider shadow;
    [SerializeField] Light lt;
    [SerializeField] string SpotEnableSound;
    [SerializeField] string SpotDisableSound;
    [SerializeField] string sonPass;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        lt.enabled = true;
        AkSoundEngine.PostEvent(SpotEnableSound, gameObject);
        yield return new WaitForSeconds(1);
        shadow.Trigger();
        yield return new WaitForSeconds(0.5f);
        AkSoundEngine.PostEvent(sonPass, gameObject);
        yield return new WaitForSeconds(2);
        lt.enabled = false;
        AkSoundEngine.PostEvent(SpotDisableSound, gameObject);
        Destroy(gameObject);
    }
}
