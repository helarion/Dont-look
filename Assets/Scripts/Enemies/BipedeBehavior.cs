using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeBehavior : Enemy
{
    [SerializeField] private float walkShakeIntensity=0.3f;
    [SerializeField] private float walkShakeDuration = 1;
    [SerializeField] string litSound;
    [SerializeField] string playRespirationSound;
    [SerializeField] string stopRespirationSound;
    [SerializeField] Vector3 rotation;

    [SerializeField] GameObject bipedeGameObject;

    private float currentWalkIntensity;
    private BoxCollider detectZone;
    private bool canSeePlayer = false;
    public bool isStopped = false;

    bool betweenSteps = false; // sert à savoir si l'on peut bouger ou pas, en accord avec l'animation
    bool willPlayPresence = true;

    [SerializeField] float retreatDistance = 0.5f;
    float retreatTime = 0.0f;

    void Start()
    {
        Initialize();
        agent.speed = 0.0f;
        detectZone = GetComponent<BoxCollider>();
    }

    void Update()
    {
        //print(betweenSteps + " " + agent.speed);
        VelocityCount();
        if (isChasing)
        {
            ChaseBehavior();
        }
    }

    public override void PlayWalk()
    {
        base.PlayWalk();
        GameManager.instance.ShakeScreen(walkShakeDuration, currentWalkIntensity);

        if (willPlayPresence)
        {
            AkSoundEngine.PostEvent(ChaseSoundPlay, gameObject);
            willPlayPresence = false;
        }
        else willPlayPresence = true;
        betweenSteps = !betweenSteps; // on inverse la valeur de betweenSteps, qui commence à false, passe à true entre 2 pas, puis à false le temps que l'anim boucle
    }

    public void Retreat()
    {
        base.PlayWalk();
        GameManager.instance.ShakeScreen(walkShakeDuration, currentWalkIntensity);
        if (willPlayPresence)
        {
            AkSoundEngine.PostEvent(ChaseSoundPlay, gameObject);
            willPlayPresence = false;
        }
        else willPlayPresence = true;
        retreatTime = 0.1f;
    }

    public override void PlayChase()
    {
        AkSoundEngine.PostEvent(playRespirationSound,gameObject);
    }

    public override void StopChaseSounds()
    {
        base.StopChaseSounds();
        AkSoundEngine.PostEvent(stopRespirationSound,gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            canSeePlayer = true;
            if (!isChasing) StartChase();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            canSeePlayer = false;
        }
    }

    public override void IsPathInvalid()
    {
        //print("ispathinvalid");
        if (canSeePlayer) return;
        //print("cannotSeePlayer");
        base.IsPathInvalid();
        //print("ispathinvalid");

    }

    public override void ChaseBehavior()
    {
        base.ChaseBehavior();
        IsPathInvalid();
        IsLit(GameManager.instance.LightDetection(bipedeGameObject, false));
        float distanceMax = (detectZone.bounds.size.x / 2);
        float rate = playerDistance.Remap(0, distanceMax, 0.2f, 1.2f);
        currentWalkIntensity = walkShakeIntensity * rate;

        if(isMoving)
        {
            // goes to the player
            MoveTo(GameManager.instance.player.transform.position);
        }
    }

    // APPELER LORSQUE LE BIPEDE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            isLooked = true;
            if (GameManager.instance.player.GetConcentration()) LitConcentrated();
            else LitNormal();
        }
        else
        {
            if (betweenSteps)
            {
                agent.speed = moveSpeed;
            }
            else
            {
                agent.speed = 0.0f;
            }
            isStopped = false;
            isLooked = false;
            agent.isStopped = false;
            isMoving = true;
            animator.SetBool("IsLooked", false);
            animator.SetBool("IsMoving", isMoving);
        }
    }

    private void LitNormal()
    {
        agent.isStopped = false;
        isMoving = true;
        isStopped = false;
        if (betweenSteps)
        {
            agent.speed = moveSpeed - malusSpeed;
        }
        else
        {
            agent.speed = 0.0f;
        }
        animator.SetBool("IsLooked", false);
        animator.SetBool("IsMoving", isMoving);
    }

    private void LitConcentrated()
    {
        if(!isStopped) AkSoundEngine.PostEvent(LookSound, gameObject);
        agent.isStopped = true;
        isStopped = true;
        isMoving = false;
        animator.SetBool("IsLooked", true);
        animator.SetBool("IsMoving", isMoving);
        if (retreatTime > 0.0f)
        {
            retreatTime -= Time.deltaTime;
            transform.position += Vector3.ClampMagnitude(transform.position - GameManager.instance.player.transform.position, retreatDistance * Time.deltaTime);
        }
    }

    public override void Respawn()
    {
        base.Respawn();
        hasPlayedLook = false;
        isLooked = false;
        betweenSteps = false;
        agent.speed = 0.0f;
        agent.isStopped = true;
        isStopped = true;
        isMoving = false;
        animator.SetBool("IsLooked", false);
        animator.SetBool("IsMoving", false);
        transform.eulerAngles = rotation;
    }
}
