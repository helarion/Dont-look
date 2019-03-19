using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 1;
    Vector3 SpawnZone;
    [HideInInspector] public NavMeshAgent agent;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        SpawnZone = transform.position;
        agent.speed = moveSpeed;
    }

    public virtual void DetectPlayer(bool b)
    {

    }

    public virtual void isLit(bool b)
    {

    }

    public virtual void Respawn()
    {
        // AJOUTER LE SPAWN ALEATOIRE DANS UNE ZONE
        transform.position = SpawnZone;
    }
}
