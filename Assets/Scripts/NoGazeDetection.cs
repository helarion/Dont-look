//-----------------------------------------------------------------------
// Copyright 2016 Tobii AB (publ). All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;
using Tobii.Gaming;

/// <summary>
/// Enable a set of UI elements if there is no gaze detected
/// </summary>
/// <remarks>
/// Referenced by the No Gaze Tracked visualization in the Eye Tracking Data example scene.
/// </remarks>
public class NoGazeDetection : MonoBehaviour
{

    PlayerController pc;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }
    void Update()
    {
        if (!TobiiAPI.GetGazePoint().IsRecent())
        {
            ClosedEyes(true);
        }
        else
        {
            ClosedEyes(false);
        }
    }

    private void ClosedEyes(bool isClosed)
    {
        pc.LightEnabled(!isClosed);
    }
}
