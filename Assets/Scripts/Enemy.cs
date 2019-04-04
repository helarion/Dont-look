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
    [SerializeField] GameObject spawnZones=null ;


    [HideInInspector] public NavMeshAgent agent;
    List<Transform> listSpawnZones;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
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

    public virtual void DetectPlayer(bool b)
    {

    }

    public virtual void IsLit(bool b)
    {

    }

    public virtual void Respawn()
    {
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
