using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Objet
{
    [SerializeField] Transform endPos;
    [SerializeField] float moveSpeed;
    [SerializeField] string activateSound;
    [SerializeField] string engineSound;
    [SerializeField] string movingSound;
    [SerializeField] string stopSound;
    [SerializeField] BoxCollider enterCol;
    private Vector3 startPos;
    //private Rigidbody rb;

    private bool isMoving = false;

    private void Start()
    {
        startPos = transform.position;
        //rb = GetComponent<Rigidbody>();
    }

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        enterCol.enabled = true;
        //AkSoundEngine.PostEvent(activateSound, gameObject);
        //AkSoundEngine.PostEvent(engineSound, gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        StopCoroutine("MoveCoroutine");
        transform.position = startPos;
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine("MoveCoroutine");
        }
    }

    IEnumerator MoveCoroutine()
    {
        GameManager.instance.player.StopMove();
        while(transform.position.y<endPos.position.y)
        {
            //rb.MovePosition(new Vector3(0, 1 * Time.deltaTime * moveSpeed, 0));
            transform.position += transform.up * Time.deltaTime*moveSpeed;
            //transform.position = Vector3.Lerp(startPos, endPos.position, Time.deltaTime*moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        //AkSoundEngine.PostEvent(stopSound, gameObject);
        GameManager.instance.player.ResumeMove();
        isMoving = false;
        yield return null;
    }
}
