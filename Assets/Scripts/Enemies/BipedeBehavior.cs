using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeBehavior : Enemy
{
    private Vector3 lastPosition;
    private bool isMoving=false;
    private bool isLooked = false;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        lastPosition = transform.position;
        agent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        isMoving = false;
        velocity = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;

        animator.SetFloat("Velocity", velocity);

        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if (isChasing)
        {
            LightDetection();
            ChaseBehavior();
        }

        //DebugPath();
        animator.SetBool("IsMoving", isMoving);
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

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            AkSoundEngine.PostEvent(WwiseLook.Id, gameObject);
            GameManager.instance.ShakeScreen(0.01f);
            isLooked = true;
        }
        else
        {
            isLooked = false;
        }
    }

}
