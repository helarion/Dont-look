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
    [SerializeField] public List<SpatialLine> spatialLines;
    [HideInInspector] public List<SpatialLine> _spatialLines;
    [SerializeField] public float newZ = -5.5f;
    [SerializeField] public float newLightRange = 16;
    [SerializeField] public float newOffset = 2;
    [SerializeField] public float newDutchAngle = 0;
    [SerializeField] public float newLightAngle = 60;
    [SerializeField] public float newContrePlongeeAngle = 20;
    [SerializeField] public float newContrePlongeeHauteur = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        _spatialLines = new List<SpatialLine>();
        foreach (SpatialLine sl in spatialLines)
        {
            bool inserted = false;
            foreach (SpatialLine _sl in _spatialLines)
            {
                if (sl.begin.position.z < _sl.begin.position.z)
                {
                    _spatialLines.Insert(spatialLines.IndexOf(_sl), sl);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                _spatialLines.Add(sl);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        foreach (SpatialLine sl in spatialLines)
        {
            Gizmos.DrawLine(sl.begin.position, sl.end.position);
        }
    }

    public void addSpatialLine(SpatialLine sl)
    {
        bool inserted = false;
        foreach (SpatialLine _sl in _spatialLines)
        {
            if (sl.begin.position.z < _sl.begin.position.z)
            {
                _spatialLines.Insert(spatialLines.IndexOf(_sl), sl);
                inserted = true;
                break;
            }
        }
        if (!inserted)
        {
            _spatialLines.Add(sl);
        }
    }
}
