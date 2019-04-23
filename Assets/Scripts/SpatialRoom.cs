using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpatialLine
{
    public Transform begin;
    public Transform end;
}

public class SpatialRoom : MonoBehaviour
{
    [SerializeField] Collider roomCollider;
    [SerializeField] public SpatialLine defaultSpatialLine;
    [SerializeField] public List<SpatialLine> additionalSpatialLines;

    public List<SpatialLine> spatialLines;


    // Start is called before the first frame update
    void Start()
    {
        spatialLines = new List<SpatialLine>();
        spatialLines.Add(defaultSpatialLine);
        foreach (SpatialLine asl in additionalSpatialLines)
        {
            foreach (SpatialLine sl in spatialLines)
            {
                if (asl.begin.position.z < sl.begin.position.z)
                {
                    spatialLines.Insert(spatialLines.IndexOf(sl), asl);
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(defaultSpatialLine.begin.position, defaultSpatialLine.end.position);
        foreach (SpatialLine sl in additionalSpatialLines)
        {
            Gizmos.DrawLine(sl.begin.position, sl.end.position);
        }
    }
}
