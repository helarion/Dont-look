using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialSas : MonoBehaviour
{
    [SerializeField] Collider sasCollider;
    [SerializeField] public SpatialLine spatialLine;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawLine(spatialLine.begin.position, spatialLine.end.position);
        }
        else
        {
            if (spatialLine != GameManager.instance.player.currentSpatialLine)
            {
                Gizmos.DrawLine(spatialLine.begin.position, spatialLine.end.position);
            }
        }
    }
}
