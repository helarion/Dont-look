using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] public float moveSpeed = 1;
    [SerializeField] public float bonusSpeed = 1;
    [SerializeField] private float velocityMax = 2;

    [Header("Chase Variables")]
    [SerializeField] private bool delete = false;
    [SerializeField] public float lookShakeIntensity=0.08f;
    [SerializeField] private float lookShakeTime = 1;
    [SerializeField] private float minChaseShakeIntensity=0.02f;
    [SerializeField] private float maxChaseShakeIntensity = 0.06f;
    [SerializeField] private float chaseShakeTime = 1;
    [SerializeField] public float delayChase = 3;
    [HideInInspector] public float playerDistance;

    private bool hasPlayedChase = false;
    [HideInInspector] public bool hasPlayedLook = false;
    [HideInInspector] public bool isChasing = false;

    [Header("Debug")]
    [SerializeField] private Transform[] spawnZones=null ;
    [SerializeField] private string WwiseChasePlay;
    [SerializeField] private string WwiseChaseStop;
    [SerializeField] public string WwiseLook;


    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    public float velocity;
    [SerializeField] private Transform _transform;
    public bool isLooked = true;
    public bool isMoving = false;
    public bool isCountingEndChase = false;
    [SerializeField] public bool needsConcentration = false;

    [HideInInspector] public PlayerController p;

    private Vector3 lastPosition;

    private void Update()
    {
        IsLit(GameManager.instance.LightDetection(transform,needsConcentration));
        VelocityCount();
    }

    public void VelocityCount()
    {
        velocity = ((transform.position - lastPosition).magnitude * 10) * moveSpeed;
        lastPosition = transform.position;
        float rate = velocity.Remap(0, velocityMax, 0, 1);
        rate = Mathf.Clamp(rate,0, 1);
        animator.SetFloat("Velocity", rate);
    }

    private void Start()
    {
        lastPosition = transform.position;
        Initialize();
    }

    public void Initialize()
    {
        p = GameManager.instance.player;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        Respawn();
    }

    // COMMENCER LA CHASSE DU JOUEUR
    public virtual void StartChase()
    {
        if(!hasPlayedChase)
        {
            hasPlayedChase = true;
            AkSoundEngine.PostEvent(GameManager.instance.ChaseAmbPlay, GameManager.instance.gameObject);
        }
        isMoving = true;
        animator.SetBool("IsMoving", isMoving);
        isChasing = true;
    }

    public void StopChase()
    {
        if (isChasing)
        {
            //AkSoundEngine.PostEvent(WwiseChaseStop.Id, gameObject);
            AkSoundEngine.PostEvent(GameManager.instance.ChaseAmbStop, GameManager.instance.gameObject);
            if(delete && p.GetIsHidden())
            {
                GameManager.instance.DeleteEnemyFromList(this);
                Destroy(gameObject);
            }
            else Respawn();
        }
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE PASSE A CHASSER LE JOUEUR. POTENTIELLEMENT INUTILE ?
    private IEnumerator CountEndChase()
    {
        print("Start Counting chase");
        float countChase = 0;
        while (countChase < delayChase)
        {
            print("count:"+countChase);
            countChase += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            //print(countChase);
        }
        print("Fin du compte");
        StopChase();
        isCountingEndChase = false;
        yield return null;
    }

    public virtual void DetectPlayer(bool b) {}

    public virtual void IsLit(bool b)
    {
        if (b)
        {
            GameManager.instance.ShakeScreen(lookShakeTime, lookShakeIntensity);
            if (!hasPlayedLook)
            {
                AkSoundEngine.PostEvent(WwiseLook, gameObject);
                hasPlayedLook = true;
            }
        }
    }

    public virtual void ChaseBehavior()
    {
        playerDistance = (transform.position - p.transform.position).magnitude;
        float distanceMax = 15;
        float chaseShakeIntensity = playerDistance.Remap(0, distanceMax, maxChaseShakeIntensity, minChaseShakeIntensity);
        GameManager.instance.ShakeScreen(chaseShakeTime, chaseShakeIntensity);
        float distanceFromPlayer = (transform.position - p.transform.position).magnitude;
        AkSoundEngine.SetRTPCValue("DISTANCE_SPIDER", distanceFromPlayer);
        IsPathInvalid();
    }

    private void IsPathInvalid()
    {
        bool isPathValid = agent.CalculatePath(p.transform.position, agent.path);
        print("path status:" + agent.path.status);
        if (!isPathValid && !isCountingEndChase)
        {
            print("path invalid");
            isCountingEndChase = true;
            StartCoroutine(CountEndChase());
        }
        else if (isPathValid)
        {
            print("path valid");
            isCountingEndChase = false;
            StopCoroutine(CountEndChase());
        }
    }

    public virtual void Respawn()
    {
        AkSoundEngine.PostEvent(GameManager.instance.ChaseAmbStop, GameManager.instance.gameObject);
        isLooked = false;
        hasPlayedChase = false;
        hasPlayedLook = false;
        isMoving = false;
        isCountingEndChase = false;
        isChasing = false;
        StopChase();
        Vector3 pos = RandomSpawn();
        animator.SetBool("IsMoving", false);
        agent.Warp(pos);
        MoveTo(pos);
    }


    public void MoveTo(Vector3 newPos)
    {
        if(agent.hasPath) agent.ResetPath();
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(newPos);
            agent.isStopped = false;
        }
    }

    public Vector3 RandomSpawn()
    {
        int max = spawnZones.Length;
        int rand = Random.Range(0, max);
        return (spawnZones[rand].position);
    }
}
