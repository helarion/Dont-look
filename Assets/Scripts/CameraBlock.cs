using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBlock : MonoBehaviour
{
    public enum BlockDirection { Left, Right };
    [SerializeField] public BlockDirection blockDirection;
    [SerializeField] public float maxCameraYawAngle = 15;
    [SerializeField] public float maxCameraPitchAngle = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
