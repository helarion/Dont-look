using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : Objet
{
    [SerializeField] private string playDoorMoveSound;
    [SerializeField] private string stopDoorMoveSound;
    [SerializeField] private float moveUpSpeed = 0.1f;
    [SerializeField] private float moveDownSpeed = 0.05f;
    [SerializeField] private Transform endPos;
    [SerializeField] private Transform startPosition;
    [SerializeField] private float shakeOnIntensity = 0.08f;
    [SerializeField] private float shakeOffIntensity = 0.04f;
    [SerializeField] private float minRTPC = 0;
    [SerializeField] private float minSlidingRTPC = 40;
    [SerializeField] private float maxSlidingRTPC = 100;
    [SerializeField] private float timeFadeRTPC = 1.5f;

    bool isPlayingSound = false;
    float distance;

    private void Start()
    {
        if (isBroken) enabled = false;
    }

    private void Update()
    {
        if (isActivating) ContiniousActivate();
        else ContiniousDesactivate();
    }

    public override void Activate()
    {
        if(!isActivated)
        {
            //AkSoundEngine.SetRTPCValue("Porte_RTPC_volume", 0);
            isActivated = true;
            StopCoroutine(CloseCoroutine());
            StartCoroutine(OpenCoroutine());
        }
    }

    public override void Desactivate()
    {
        if (isActivated)
        {
            isActivated = false;
            StopCoroutine(OpenCoroutine());
            StartCoroutine(CloseCoroutine());
        }
    }

    IEnumerator OpenCoroutine()
    {
        distance = (transform.localPosition - endPos.localPosition).magnitude;
        while (distance > 0.1f)
        {
            transform.position += (transform.up) * Time.deltaTime * moveUpSpeed;
            distance = (transform.position - endPos.position).magnitude;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos.position;
        //AkSoundEngine.PostEvent(stopSound, gameObject);
        //GameManager.instance.player.ResumeMove();
        yield return null;
    }

    IEnumerator CloseCoroutine()
    {
        distance = (transform.localPosition -startPosition.localPosition).magnitude;
        while (distance > 0.1f)
        {
            transform.position += (transform.up*-1) * Time.deltaTime * moveDownSpeed;
            distance = (transform.position - endPos.position).magnitude;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos.position;
        //AkSoundEngine.PostEvent(stopSound, gameObject);
        //GameManager.instance.player.ResumeMove();
        yield return null;
    }

    private void ContiniousActivate()
    {
        distance = endPos.localPosition.y-transform.localPosition.y;
        if (distance > 0.01f)
        {
            if (!isPlayingSound)
            {
                isPlayingSound = true;
                AkSoundEngine.PostEvent(playDoorMoveSound, gameObject);
            }
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up) * Time.deltaTime * moveUpSpeed;
        }
        else
        {
            transform.localPosition = endPos.localPosition;
            AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
            isPlayingSound = false;
        }
    }

    private void ContiniousDesactivate()
    {
        distance = transform.localPosition.y - startPosition.localPosition.y;
        if (distance > 0.01f)
        {
            if (!isPlayingSound)
            {
                isPlayingSound = true;
                AkSoundEngine.PostEvent(playDoorMoveSound, gameObject);
            }
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up * -1) * Time.deltaTime * moveDownSpeed;
        }
        else
        {
            transform.localPosition = startPosition.localPosition;
            AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
            isPlayingSound = false;
        }
    }

    public override void Break()
    {
        transform.localPosition = endPos.localPosition;
        enabled = false;
        base.Break();
    }

    public override void Reset()
    {
        base.Reset();
        //isActivated = false;
        isActivating = false;
        AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
        if(isBroken)
        {
            Break();
        }
        else transform.localPosition = startPosition.localPosition;
    }
}
