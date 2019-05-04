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
    Collider col;

    // Start is called before the first frame update
    void Start()
    {
        occupied = false;
        exited = false;
        exitRtpcValue = false;
        col = gameObject.GetComponent<Collider>();
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
                exitRtpcValue = playerPosition.y < col.bounds.min.y;
            }
            else if (direction == Direction.Up)
            {
                exitRtpcValue = playerPosition.y > col.bounds.max.y;
            }
            else if (direction == Direction.Left)
            {
                exitRtpcValue = playerPosition.x > col.bounds.max.x;
            }
            else if (direction == Direction.Right)
            {
                exitRtpcValue = playerPosition.x < col.bounds.min.x;
            }
        }
    }

    public Collider getCollider()
    {
        return col;
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
