using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialRoom : MonoBehaviour
{
    [System.Serializable]
    public class SpatialLine
    {
        public Transform begin;
        public Transform end;
    }

    [SerializeField] Collider roomCollider;
    [SerializeField] SpatialLine defaultSpatialLine;
    [SerializeField] List<SpatialLine> additionalSpatialLines;

    List<SpatialLine> spatialLines;


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

        foreach (SpatialLine sl in spatialLines)
        {
            print(sl.begin.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
