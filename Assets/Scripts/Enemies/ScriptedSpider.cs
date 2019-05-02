using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptedSpider : Enemy
{
    [SerializeField] Transform pos1;
    [SerializeField] Transform pos2;
    [SerializeField] ScriptLamp lamp;
    [SerializeField] BoxCollider ladderCol;
    [SerializeField] int nbAttack = 2;
    [SerializeField] float scriptIntensity;
    [SerializeField] private bool hasPlayedLook = false;

    private bool objective1 = false;
    private bool objective2 = false;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        VelocityCount();
        LightDetection();
        if (isChasing)
        {
            ChaseBehavior();
        }

        if (agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
            agent.isStopped = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DetectZone"))
        {
            GameManager.instance.ShakeScreen(0.2f,scriptIntensity);
            lamp.Swing();
        }
    }

    public void Script()
    {
        agent.speed = moveSpeed;
        StartChase();
        StartCoroutine("ScriptRoutine");
    }

    private IEnumerator ScriptRoutine()
    {
        int count = 0;
        int step = 1;
        while (count <nbAttack)
        {
            if (step == 1) MoveTo(pos1.position);
            else if(step==2) MoveTo(pos2.position);
            else
            {
                count++;
                step = 1;
                if (count < nbAttack)
                {
                    MoveTo(pos1.position);
                }
            }
            yield return new WaitForSeconds(0.5f);
            while (agent.remainingDistance >0)
            {
                yield return new WaitForSeconds(0.1f);
            }
            step++;
            yield return new WaitForEndOfFrame();
        }
        ladderCol.isTrigger = true;
        StopChase();
        GameManager.instance.DeleteEnemyFromList(this);
        Destroy(gameObject);        
        yield return null;
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            if (!hasPlayedLook)
            {
                AkSoundEngine.PostEvent(WwiseLook, gameObject);
                hasPlayedLook = true;
            }
            GameManager.instance.ShakeScreen(0.1f, shakeIntensity);
        }
    }
}
