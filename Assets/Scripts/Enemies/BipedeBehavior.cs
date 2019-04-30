using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeBehavior : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        agent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        VelocityCount();

        if (isChasing)
        {
            LightDetection();
            ChaseBehavior();
        }

        //DebugPath();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        StartChase();
    }

    public override void ChaseBehavior()
    {
        if(!isLooked)
        {
            // goes to the player
            isMoving = true;
            MoveTo(GameManager.instance.player.transform.position);
        }
        else
        {
            print("regardé");
            isMoving = false;
            agent.isStopped = true;
        }
    }

    // APPELER LORSQUE LE BIPEDE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            //AkSoundEngine.PostEvent(WwiseLook.Id, gameObject);
            GameManager.instance.ShakeScreen(0.001f);
            isLooked = true;
            animator.SetBool("IsLooked", true);
        }
        else
        {
            isLooked = false;
            animator.SetBool("IsLooked", false);
        }
    }

}
