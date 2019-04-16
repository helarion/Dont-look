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

    [Header("Debug")]
    [SerializeField] private GameObject spawnZones=null ;
    [SerializeField] private AK.Wwise.Event WwiseChase;
    [SerializeField] public AK.Wwise.Event WwiseLook;


    [HideInInspector] public NavMeshAgent agent;

    private List<Transform> listSpawnZones;
    [HideInInspector] public Animator animator;
    public float velocity;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        listSpawnZones = new List<Transform>();
        Transform[] temp = spawnZones.GetComponentsInChildren<Transform>();
        int a = 0;
        foreach (Transform t in temp)
        {
            if(a!=0) listSpawnZones.Add(t);
            a++;
        }

        Respawn();
    }

    // COMMENCER LA CHASSE DU JOUEUR
    public virtual void StartChase()
    {
        AkSoundEngine.PostEvent(WwiseChase.Id,gameObject);
        agent.isStopped = false;
        isChasing = true;
    }

    public virtual void DetectPlayer(bool b) {}
    public virtual void IsLit(bool b) {}
    public virtual void ChaseBehavior() {}

    public void LightDetection()
    {
        bool test = false;
        Vector3 playerPosition = GameManager.instance.player.transform.position;
        Vector3 lightVec = GameManager.instance.player.GetLookAt() - playerPosition;
        Vector3 playerToSpiderVec = transform.position - GameManager.instance.player.transform.position;

        float playerToSpiderLength = playerToSpiderVec.magnitude;
        Light playerLight = GameManager.instance.player.getLight();
        float lightRange = playerLight.range;
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
        isChasing = false;
        Vector3 pos = RandomSpawn();
        transform.position = pos;
        MoveTo(pos);
    }

    public void MoveTo(Vector3 newPos)
    {
        agent.ResetPath();
        agent.SetDestination(newPos);
        agent.isStopped = false;
    }

    public Vector3 RandomSpawn()
    {
        int max = listSpawnZones.Count;
        int rand = Random.Range(0, max);
        return (listSpawnZones[rand].position);
    }
}
