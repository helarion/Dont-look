using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] public float moveSpeed = 1;

    [Header("Chase Variables")]
    [SerializeField] public bool isChasing = false;
    [SerializeField] private bool delete = false;
    [SerializeField] public float lookShakeIntensity=0.08f;
    private bool hasPlayedChase = false;
    [SerializeField] private float chaseShakeIntensity=0.02f;
    [SerializeField] private float delayBeforeChase = 1;
    [SerializeField] public float delayChase = 3;

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

    [HideInInspector] public PlayerController p;

    private Vector3 lastPosition;

    private void Update()
    {
        VelocityCount();
    }

    public void VelocityCount()
    {
        velocity = ((transform.position - lastPosition).magnitude * 10) * moveSpeed;
        lastPosition = transform.position;
        animator.SetFloat("Velocity", velocity);
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
        StartCoroutine("WaitBeforeChaseCoroutine");
    }

    IEnumerator WaitBeforeChaseCoroutine()
    {
        agent.speed = 1;
        agent.isStopped = false;
        isChasing = true;
        isMoving = true;
        animator.SetBool("IsMoving", isMoving);
        yield return new WaitForSeconds(delayBeforeChase);
        agent.speed = moveSpeed;
        //AkSoundEngine.PostEvent(WwiseChasePlay.Id,gameObject);
    }

    public void StopChase()
    {
        if (isChasing)
        {
            isMoving = false;
            isChasing = false;
            animator.SetBool("IsMoving", isMoving);
            //AkSoundEngine.PostEvent(WwiseChaseStop.Id, gameObject);
            AkSoundEngine.PostEvent(GameManager.instance.ChaseAmbStop, GameManager.instance.gameObject);
            //AkSoundEngine.PostEvent(GameManager.instance.HeartStop, GameManager.instance.player.gameObject);
            //GameManager.instance.PlayHeart();
            if (!delete) Respawn();
            else if (p.GetIsHidden())
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
    public virtual void IsLit(bool b) {}
    public virtual void ChaseBehavior()
    {
        GameManager.instance.ShakeScreen(0.01f, chaseShakeIntensity);
        float distanceFromPlayer = (transform.position - p.transform.position).magnitude;
        AkSoundEngine.SetRTPCValue("DISTANCE_SPIDER", distanceFromPlayer);
        bool isPathValid = agent.CalculatePath(p.transform.position, agent.path);
        if(!isPathValid && !isCountingEndChase)
        {
            print("path invalid");
            isCountingEndChase = true;
            StartCoroutine("CountEndChase");
        }
        else if(isPathValid)
        {
            print("path valid");
            isCountingEndChase = false;
            StopCoroutine("CountEndChase");
        }
    }

    public void LightDetection()
    {
        bool test = false;
        Vector3 playerPosition = GameManager.instance.player.transform.position;
        playerPosition.y += 1;
        Vector3 lightVec = GameManager.instance.player.GetLookAt() - playerPosition;
        Vector3 playerToSpiderVec = transform.position - GameManager.instance.player.transform.position;

        float playerToSpiderLength = playerToSpiderVec.magnitude;
        Light playerLight = GameManager.instance.player.getLight();
        float lightDim = GameManager.instance.player.rangeDim;
        float lightRange = playerLight.range-lightDim;
        float lightAngle = playerLight.spotAngle / 2.0f;
        if (playerToSpiderLength <= lightRange)
        {
            float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToSpiderVec) / (lightVec.magnitude * playerToSpiderVec.magnitude)) * Mathf.Rad2Deg;
            if (angleFromLight <= lightAngle)
            {
                lightVec = Vector3.RotateTowards(lightVec, playerToSpiderVec, angleFromLight, Mathf.Infinity);

                RaycastHit hit;
                Ray ray = new Ray(playerPosition, lightVec);
                Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetWallsAndMobsLayer());

                if (hit.transform.gameObject.tag == gameObject.tag)
                {
                    test = true;
                }
                //print("Touched " + hit.transform.gameObject.name);
            }
        }
        IsLit(test);
    }

    public virtual void Respawn()
    {
        hasPlayedChase = false;
        agent.speed = 75;
        StopChase();
        Vector3 pos = RandomSpawn();
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
