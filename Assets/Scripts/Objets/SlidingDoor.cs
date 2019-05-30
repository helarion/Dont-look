using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : Objet
{
    [SerializeField] private string playDoorMoveSound;
    [SerializeField] private string stopDoorMoveSound;
    [SerializeField] private string playDoorHitsWallSound;

    [SerializeField] private float moveUpSpeed = 0.1f;
    [SerializeField] private float moveDownSpeed = 0.05f;
    [SerializeField] private Transform endPos;
    [SerializeField] private Transform startPosition;
    [SerializeField] private float shakeOnIntensity = 0.08f;
    [SerializeField] private float shakeOffIntensity = 0.04f;
    [SerializeField] private float minRTPC = 0;
    [SerializeField] private float minSlidingRTPC = 40;
    [SerializeField] private float maxSlidingRTPC = 100;
    [SerializeField] private float fadeRTPCDuration = 1.5f;
    [SerializeField] private string rtpcName = null;
    float timeFadeRTPC = 0.0f;
    float slideRtpc = 0.0f;
    float startRtpc = 0.0f;

    bool isPlayingSound = false;
    float distance;

    List<bool> setStates = new List<bool>();
    bool forcedState = false;

    bool broken = false;

    enum SlidingDoorState
    {
        CLOSED,
        OPENED,
        GOING_UP,
        GOING_DOWN
    }

    SlidingDoorState slidingDoorState = SlidingDoorState.CLOSED;
    SlidingDoorState saveState;

    private void Start()
    {
        AkSoundEngine.SetRTPCValue(rtpcName, 0.0f);
        if (isBroken)
        {
            Break();
        }
    }

    private void Update()
    {
        if (broken) return;
        update_state();
        if (slidingDoorState == SlidingDoorState.GOING_UP)
        {
            distance = endPos.localPosition.y - transform.localPosition.y;
            if (distance > 0.01f)
            {
                if (!isPlayingSound)
                {
                    isPlayingSound = true;
                    AkSoundEngine.PostEvent(playDoorMoveSound, gameObject);
                    startRtpc = slideRtpc;
                    timeFadeRTPC = fadeRTPCDuration * (startRtpc / maxSlidingRTPC);
                }
                if (timeFadeRTPC < fadeRTPCDuration)
                {
                    timeFadeRTPC += Time.deltaTime;
                    slideRtpc = (timeFadeRTPC / fadeRTPCDuration) * maxSlidingRTPC;
                    AkSoundEngine.SetRTPCValue(rtpcName, slideRtpc);
                }
                else
                {
                    timeFadeRTPC = fadeRTPCDuration;
                    slideRtpc = maxSlidingRTPC;
                    AkSoundEngine.SetRTPCValue(rtpcName, slideRtpc);
                }
                GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
                transform.localPosition += (transform.up) * Time.deltaTime * moveUpSpeed;
            }
            else
            {
                transform.localPosition = endPos.localPosition;
                AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
                AkSoundEngine.PostEvent(playDoorHitsWallSound, gameObject);
                isPlayingSound = false;
                slidingDoorState = SlidingDoorState.OPENED;
            }
        }
        else if (slidingDoorState == SlidingDoorState.GOING_DOWN)
        {
            distance = transform.localPosition.y - startPosition.localPosition.y;
            if (distance > 0.01f)
            {
                if (!isPlayingSound)
                {
                    isPlayingSound = true;
                    AkSoundEngine.PostEvent(playDoorMoveSound, gameObject);
                    startRtpc = slideRtpc;
                    timeFadeRTPC = fadeRTPCDuration * (Mathf.Abs(startRtpc - minSlidingRTPC) / (maxSlidingRTPC - minSlidingRTPC));
                }
                if (timeFadeRTPC < fadeRTPCDuration)
                {
                    timeFadeRTPC += Time.deltaTime;
                    slideRtpc = Mathf.Lerp(startRtpc, minSlidingRTPC, timeFadeRTPC);
                }
                else
                {
                    timeFadeRTPC = fadeRTPCDuration;
                    slideRtpc = minSlidingRTPC;
                    AkSoundEngine.SetRTPCValue(rtpcName, slideRtpc);
                }
                GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
                transform.localPosition += (transform.up * -1) * Time.deltaTime * moveDownSpeed;
            }
            else
            {
                transform.localPosition = startPosition.localPosition;
                AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
                AkSoundEngine.PostEvent(playDoorHitsWallSound, gameObject);
                isPlayingSound = false;
                slidingDoorState = SlidingDoorState.CLOSED;
            }
        }
    }

    public void Stop()
    {
        saveState = slidingDoorState;
        slidingDoorState = SlidingDoorState.CLOSED;
    }

    public void Resume()
    {
        slidingDoorState = saveState;
    }

    public void set_activation_state(bool state)
    {
        setStates.Add(state);
    }

    public void force_activation_state(bool state)
    {
        isActivating = state;
        forcedState = true;
    }

    public void update_state()
    {
        if (!forcedState)
        {
            if (setStates.Count != 0)
            {
                isActivating = false;
            }
            foreach (bool state in setStates)
            {
                if (state)
                {
                    isActivating = true;
                    break;
                }
            }
        }
        else
        {
            forcedState = false;
        }
        setStates.Clear();

        if (isActivating && slidingDoorState != SlidingDoorState.GOING_UP && slidingDoorState != SlidingDoorState.OPENED)
        {
            Activate();
        }
        else if (!isActivating && (slidingDoorState == SlidingDoorState.GOING_UP || slidingDoorState == SlidingDoorState.OPENED))
        {
            Desactivate();
        }
    }

    public override void Activate()
    {
        slidingDoorState = SlidingDoorState.GOING_UP;
        AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
        isPlayingSound = false;
    }

    public override void Desactivate()
    {
        slidingDoorState = SlidingDoorState.GOING_DOWN;
        AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
        isPlayingSound = false;
    }

    public override void Break()
    {
        broken = true;
        transform.localPosition = endPos.localPosition;
        enabled = false;
        base.Break();
        slidingDoorState = SlidingDoorState.OPENED;
    }

    public override void Reset()
    {
        base.Reset();
        timeFadeRTPC = 0.0f;
        slideRtpc = 0.0f;
        startRtpc = 0.0f;
        AkSoundEngine.SetRTPCValue(rtpcName, 0.0f);
        //isActivated = false;
        isActivating = false;
        forcedState = false;
        AkSoundEngine.PostEvent(stopDoorMoveSound, gameObject);
        if (isBroken)
        {
            Break();
        }
        else
        {
            transform.localPosition = startPosition.localPosition;
            slidingDoorState = SlidingDoorState.CLOSED;
        }
    }

    public override void Fix()
    {
        base.Fix();
        broken = false;
    }
}
