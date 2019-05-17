using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptedSpider : Enemy
{
    [SerializeField] Transform pos1;
    [SerializeField] Transform pos2;
    [SerializeField] ScriptLamp lamp;
    [SerializeField] int nbAttack = 2;
    [SerializeField] float scriptShakeIntensity;
    [SerializeField] float scriptShakeDuration = 1;
    [SerializeField] float waitTime=1;

    private bool objective1 = false;
    private bool objective2 = false;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        VelocityCount();
        IsLit(GameManager.instance.LightDetection(gameObject, false));
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
            GameManager.instance.ShakeScreen(scriptShakeDuration,scriptShakeIntensity);
            lamp.Swing();
        }
    }

    public void Script()
    {
        agent.speed = moveSpeed;
        StartChase();
        StartCoroutine(ScriptRoutine());
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
                yield return new WaitForSeconds(waitTime);
            }
            step++;
            yield return new WaitForEndOfFrame();
        }
        StopChase();
        GameManager.instance.DeleteEnemyFromList(this);
        Destroy(gameObject);        
        yield return null;
    }
}
