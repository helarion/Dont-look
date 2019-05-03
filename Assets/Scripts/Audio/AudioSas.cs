using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSas : MonoBehaviour
{
    public enum Direction { Left, Right, Down, Up };
    [SerializeField] public Direction direction;
    bool occupied;
    bool exited;
    bool exitRtpcValue;
    Collider collider;

    // Start is called before the first frame update
    void Start()
    {
        occupied = false;
        exited = false;
        exitRtpcValue = false;
        collider = gameObject.GetComponent<Collider>();
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
            Vector3 playerPosition = other.transform.position;
            occupied = false;
            exited = true;
            if (direction == Direction.Down)
            {
                exitRtpcValue = playerPosition.y < collider.bounds.min.y;
            }
            else if (direction == Direction.Up)
            {
                exitRtpcValue = playerPosition.y > collider.bounds.max.y;
            }
            else if (direction == Direction.Left)
            {
                exitRtpcValue = playerPosition.x > collider.bounds.max.x;
            }
            else if (direction == Direction.Right)
            {
                exitRtpcValue = playerPosition.x < collider.bounds.min.x;
            }
        }
    }

    public Collider getCollider()
    {
        return collider;
    }

    public bool getIsOccupied()
    {
        return occupied;
    }

    public bool getIsExited()
    {
        if (exited)
        {
            exited = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool getExitRtpcValue()
    {
        return exitRtpcValue;
    }
}
