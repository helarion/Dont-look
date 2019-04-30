using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptedSpider : Enemy
{
    [SerializeField] Transform pos1;
    [SerializeField] Transform pos2;
    [SerializeField] ScriptLamp lamp;
    [SerializeField] float shakeIntensity;
    [SerializeField] BoxCollider ladderCol;
    [SerializeField] int nbAttack = 2;

    private bool objective1 = false;
    private bool objective2 = false;
    float save;

    private void Start()
    {
        Initialize();
        save = GameManager.instance.GetShakeIntensity();
    }

    private void Update()
    {
        VelocityCount();
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
            GameManager.instance.SetShakeIntensity(shakeIntensity);
            GameManager.instance.ShakeScreen(0.2f);
            lamp.Swing();
        }
    }

    public void Script()
    {
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
            yield return new WaitForSeconds(0.2f);
            while (agent.remainingDistance >0)
            {
                //print("remaining distance:"+agent.remainingDistance);
                //print("Step" + step);
                yield return new WaitForSeconds(0.1f);
            }
            step++;
            yield return new WaitForEndOfFrame();
        }
        ladderCol.isTrigger = true;
        StopChase();
        GameManager.instance.SetShakeIntensity(save);
        Destroy(gameObject);        
        yield return null;
    }
}
