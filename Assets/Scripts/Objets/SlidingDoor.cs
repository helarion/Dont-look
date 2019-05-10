using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : Objet
{
    [SerializeField] private string doorOpenSound;
    [SerializeField] private float moveUpSpeed = 0.1f;
    [SerializeField] private float moveDownSpeed = 0.05f;
    [SerializeField] private Transform endPos;
    [SerializeField] private Transform startPosition;
    [SerializeField] private float shakeOnIntensity = 0.08f;
    [SerializeField] private float shakeOffIntensity = 0.04f;

    float distance;

    private void Start()
    {
        if (isBroken) this.enabled = false;
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
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up) * Time.deltaTime * moveUpSpeed;
        }
        else transform.localPosition = endPos.localPosition;
    }

    private void ContiniousDesactivate()
    {
        distance = transform.localPosition.y - startPosition.localPosition.y;
        if (distance > 0.01f)
        {
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up * -1) * Time.deltaTime * moveDownSpeed;
        }
        else transform.localPosition = startPosition.localPosition;
    }

    public override void Break()
    {
        transform.localPosition = endPos.localPosition;
        base.Break();
    }

    public override void Reset()
    {
        base.Reset();
        isActivated = false;
        isActivating = false;
        if(isBroken)
        {
            Break();
        }
        else transform.localPosition = startPosition.localPosition;
    }
}
