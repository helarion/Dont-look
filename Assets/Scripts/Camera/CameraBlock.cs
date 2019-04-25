﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBlock : MonoBehaviour
{
    public enum BlockDirection { Left, Right, Up, Down };
    [SerializeField] public BlockDirection blockDirection;
    [SerializeField] public float maxCameraYawAngle = 15;
    [SerializeField] public float maxCameraPitchAngle = 10;
    [SerializeField] public float newZ = -5.5f;
    [SerializeField] public bool ChangesLightRange = false;
    [SerializeField] public float newLightRange = 16;
}
