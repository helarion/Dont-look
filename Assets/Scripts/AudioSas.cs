using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSas : MonoBehaviour
{
    public enum Direction { Left, Right, Down, Up };
    [SerializeField] public Direction direction;
    public bool occupied;

    // Start is called before the first frame update
    void Start()
    {
        occupied = false;
    }

    // Update is called once per frame
    void Update()
    {
        ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            occupied = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            occupied = false;
        }
    }

    public Collider getCollider()
    {
        return gameObject.GetComponent<Collider>();
    }
}
