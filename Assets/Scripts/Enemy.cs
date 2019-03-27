﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 1;
    [HideInInspector] public NavMeshAgent agent;
    [SerializeField] GameObject spawnZones;

    public bool isChasing = false;

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

    public virtual void isLit(bool b)
    {

    }

    public virtual void Respawn()
    {
        Vector3 pos = RandomSpawn();
        transform.position = pos;
        //agent.SetDestination(pos);
    }

    public Vector3 RandomSpawn()
    {
        int max = listSpawnZones.Count;
        int rand = Random.Range(0, max);
        return (listSpawnZones[rand].position);
    }
}
