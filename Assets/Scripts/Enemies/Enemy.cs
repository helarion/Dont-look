using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    #region Variables
    [Header("Movement Variables")]
    [SerializeField] public float moveSpeed = 1; //initial speed
    [SerializeField] public float malusSpeed = 1; //malus applied to speed when looked
    [SerializeField] private float velocityMax = 2; //maximum movement animation velocity
    private Vector3 lastPosition; //last position of the monster to calculate velocity
    public float velocity; //move animation current velocity
    public bool isMoving = false; //if the monster is moving

    [Header("Chase Variables")]
    [SerializeField] private bool delete = false; //if we want the monster to disappear after losting the child

    [SerializeField] public float lookShakeIntensity=0.08f; //intensity of the screenshake when monster looked
    [SerializeField] private float lookShakeTime = 1; //duration of the screenshake when monster looked
    [SerializeField] private float minChaseShakeIntensity=0.02f; //min screenshake when monster chasing
    [SerializeField] private float maxChaseShakeIntensity = 0.06f; //max screenshake when monster chasing
    [SerializeField] private float chaseShakeTime = 1; //duration of screenshake when monster chasing
    [SerializeField] private bool ChecksIfPathInvalid = true;

    [SerializeField] public float delayChase = 3; //delay before monster stop chase if he cannot reach player
    [HideInInspector] public float playerDistance; //distance between the monster and the player

    [HideInInspector] public bool isChasing = false; //if monster is curently chasing
    public bool isLooked = true; //if the monster is looked
    public bool isCountingEndChase = false; //if the monster cannot reach the player anymore

    [Header("Sound")]
    private bool hasPlayedChase = false; //if the chase amb has been played
    [HideInInspector] public bool hasPlayedLook = false; //if the look sound has been played
    // wwise events
    [SerializeField] public string ChaseSoundPlay; //play monster sounds when chasing
    [SerializeField] public string ChaseSoundStop; //stop 
    [SerializeField] public string LookSound; //play deep sound when monster looked
    [SerializeField] public string lookSoundScream; //play loud sound when monster looked
    [SerializeField] public string WalkSound; //play walk sound

    [Header("Gamobjects")]
    [SerializeField] public Transform[] spawnZones=null ; //list of spawn zones
    [SerializeField] private Objet[] scriptedObjectsActivation; //if spider needs to activate objets when start to chase
    [SerializeField] private LightDetector scriptedLampActivation; //if spider needs to activate lightdetector when start to chase
    [SerializeField] public float spawnDistanceFromPlayer = 10.0f;

    [HideInInspector] public Animator animator;
    [SerializeField] private Transform _transform;
    public NavMeshAgent agent;
    [HideInInspector] public PlayerController player;

    [HideInInspector] public int spawnIndex = -1;

    #endregion

    public virtual void PlayChase() { }
    public virtual void DetectPlayer(bool b) { }

    #region StartUpdate
    private void Start()
    {
        lastPosition = transform.position;
        Initialize();
    }

    private void Update()
    {
        //checks if the monster is lit
        IsLit(GameManager.instance.LightDetection(gameObject, false));
        //calculate current velocity
        VelocityCount();
    }
    #endregion

    // outputs monster velocity from previous position to current
    public void VelocityCount()
    {
        velocity = ((transform.position - lastPosition).magnitude * 10) * moveSpeed;
        lastPosition = transform.position;
        float rate = velocity.Remap(0, velocityMax, 0, 1);
        rate = Mathf.Clamp(rate,0, 1);
        animator.SetFloat("Velocity", rate);
    }

    public void Initialize()
    {
        player = GameManager.instance.player;
        animator = GetComponent<Animator>();
        agent.speed = moveSpeed;
        Respawn();
    }

    public virtual void PlayWalk()
    {
        AkSoundEngine.PostEvent(WalkSound, gameObject);
    }

    // Starts chasing player
    public virtual void StartChase()
    {
        if(!hasPlayedChase)
        {
            AkSoundEngine.PostEvent(GameManager.instance.stopRandomSounds, GameManager.instance.gameObject);
            hasPlayedChase = true;
            AkSoundEngine.PostEvent(ChaseSoundPlay, gameObject);
            PlayChase();
            isMoving = true;
            animator.SetBool("IsMoving", isMoving);
            isChasing = true;
        }
    }

    public virtual void StopChaseSounds()
    {
        AkSoundEngine.PostEvent(ChaseSoundStop, gameObject);
        if (player.getIsAlive())
        {
            AkSoundEngine.PostEvent(GameManager.instance.ChaseSpiderAmbStop, player.modelTransform.gameObject);
            AkSoundEngine.PostEvent(GameManager.instance.playRandomSounds, GameManager.instance.gameObject);
        }
        else AkSoundEngine.PostEvent(GameManager.instance.ChaseSpiderKillStop, player.modelTransform.gameObject);
    }

    // Stops chasing player
    public virtual void StopChase()
    {
        if (isChasing)
        {
            GameManager.instance.PostProcessReset();
            StopChaseSounds();
            if (delete && player.GetIsHidden())
            {
                foreach(Objet o in scriptedObjectsActivation)
                {
                    o.Activate();
                }
                GameManager.instance.DeleteEnemyFromList(this);
                Destroy(gameObject);
            }
            else Respawn();
        }
    }

    // count to delay when cannot reach player
    private IEnumerator CountEndChase()
    {
        float countChase = 0;
        while (countChase < delayChase)
        {
            countChase += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            //print(countChase);
        }
        StopChase();
        isCountingEndChase = false;
        yield return null;
    }

    public virtual void IsLit(bool b)
    {
        if (b)
        {
            GameManager.instance.ShakeScreen(lookShakeTime, lookShakeIntensity);
            if (!hasPlayedLook)
            {
                if (scriptedLampActivation != null) scriptedLampActivation.ForceLit();
                AkSoundEngine.PostEvent(LookSound, gameObject);
                AkSoundEngine.PostEvent(lookSoundScream, gameObject);
                hasPlayedLook = true;
            }
        }
    }

    public virtual void ChaseBehavior()
    {
        float distanceFromPlayer = (transform.position - player.transform.position).magnitude;
        float distanceMaxShake = 15;
        float chaseShakeIntensity = distanceFromPlayer.Remap(0, distanceMaxShake, maxChaseShakeIntensity, minChaseShakeIntensity);
        GameManager.instance.UpdatePostProcess(distanceFromPlayer);
        GameManager.instance.ShakeScreen(chaseShakeTime, chaseShakeIntensity);
        //distanceFromPlayer = GameManager.instance.maxDistRtpc - distanceFromPlayer;
        AkSoundEngine.SetRTPCValue("DISTANCE_SPIDER", distanceFromPlayer);
        IsPathInvalid();
    }

    public virtual void IsPathInvalid()
    {
        if (!ChecksIfPathInvalid) return;
        bool isPathValid = agent.CalculatePath(player.transform.position, agent.path);
        //print("isPathValid:" + isPathValid);
        //print("path status:" + agent.path.status);
        if (!isPathValid && !isCountingEndChase)
        {
            //print("path invalid");
            isCountingEndChase = true;
            StartCoroutine(CountEndChase());
        }
        else if (isPathValid)
        {
            //print("path valid");
            isCountingEndChase = false;
            StopCoroutine(CountEndChase());
        }
    }

    public virtual void Respawn(bool farOfPlayer = false)
    {
        AkSoundEngine.PostEvent(GameManager.instance.ChaseSpiderAmbStop, GameManager.instance.gameObject);
        AkSoundEngine.PostEvent(GameManager.instance.ChaseBipedeAmbStop, GameManager.instance.gameObject);
        isLooked = false;
        hasPlayedChase = false;
        hasPlayedLook = false;
        isMoving = false;
        isCountingEndChase = false;
        isChasing = false;
        StopChase();
        Vector3 pos = RandomSpawn(farOfPlayer);
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

    #region GetSet
    public Vector3 RandomSpawn(bool farOfPlayer)
    {
        int max = spawnZones.Length;
        //print("max=" + max);
        spawnIndex = Random.Range(0, max-1);
        if (farOfPlayer)
        {
            while ((spawnZones[spawnIndex].position - player.transform.position).magnitude < spawnDistanceFromPlayer)
            {
                spawnIndex = Random.Range(0, max);
            }
        }
        //print("spawnIndex:" + spawnIndex);
        return (spawnZones[spawnIndex].position);
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    public Transform[] GetSpawnZones()
    {
        return spawnZones;
    }
    #endregion
}
