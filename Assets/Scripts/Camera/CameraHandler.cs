﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Transform target=null; // target = player, le suivre avec la cam
    PlayerController player;

    [Header("Camera Variables")]
    [SerializeField] float offset=1;
    [SerializeField] float normalOffset = 1;
    [SerializeField] float zoomOffset = 1.5f;
    [SerializeField] float cameraYawAngleMultiplier = 10;
    [SerializeField] float minYCamera = 1.0f;
    [SerializeField] float railOffsetX;
    [SerializeField] float railOffsetY;
    
    public float zStartPosition;

    void Start()
    {
        zStartPosition = transform.position.z;
        player = GameManager.instance.player;
    }

    void Update()
    {
        if (target==null || GameManager.instance.GetIsPaused()) return;

        Vector3 lookAtPos = player.GetLookAt();

        Vector3 newPosition = target.position + Vector3.ClampMagnitude(lookAtPos - target.position, offset);
        //print(newPosition);
        if (newPosition.y < target.position.y-minYCamera)
        {
            newPosition.y = target.position.y - minYCamera;
        }
        newPosition.z = zStartPosition;
        
        float cameraYawAngle = player.getCursorPosNormalized().x * cameraYawAngleMultiplier;

        Quaternion newRotate = Quaternion.Euler(0, cameraYawAngle, 0);

        CameraBlock currentCameraBlock = player.getCameraBlock();
        if (currentCameraBlock != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPos - transform.position, Vector3.up);
            
            if (currentCameraBlock.blockDirection == CameraBlock.BlockDirection.Left)
            {
                if (newPosition.x < currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.x)
                {
                    newPosition.x = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.x;
                }
                
                if (targetRotation.eulerAngles.y > 0 && targetRotation.eulerAngles.y < 180)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, -targetRotation.eulerAngles.y, 0));
                }
            }
            else if (currentCameraBlock.blockDirection == CameraBlock.BlockDirection.Right)
            {
                if (newPosition.x > currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.x)
                {
                    newPosition.x = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.x;
                }
                
                if (targetRotation.eulerAngles.y > 180)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, -targetRotation.eulerAngles.y, 0));
                }
            }
            else if (currentCameraBlock.blockDirection == CameraBlock.BlockDirection.Up)
            {
                if (newPosition.y > currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.y)
                {
                    newPosition.y = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.y;
                }

                if (targetRotation.eulerAngles.x > 0 && targetRotation.eulerAngles.x < 180)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(-targetRotation.eulerAngles.x, 0, 0));
                }
            }
            else if (currentCameraBlock.blockDirection == CameraBlock.BlockDirection.Down)
            {
                if (newPosition.y < currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.y)
                {
                    newPosition.y = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.y;
                }

                if (targetRotation.eulerAngles.x > 180)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(-targetRotation.eulerAngles.x, 0, 0));
                }
            }

            if (targetRotation.eulerAngles.y < 360 - currentCameraBlock.maxCameraYawAngle && targetRotation.eulerAngles.y > 180)
            {
                targetRotation *= Quaternion.Euler(new Vector3(0, 360 - currentCameraBlock.maxCameraYawAngle - targetRotation.eulerAngles.y, 0));
            }
            else if (targetRotation.eulerAngles.y > currentCameraBlock.maxCameraYawAngle && targetRotation.eulerAngles.y < 180)
            {
                targetRotation *= Quaternion.Euler(new Vector3(0, currentCameraBlock.maxCameraYawAngle - targetRotation.eulerAngles.y, 0));
            }

            if (targetRotation.eulerAngles.x < 360 - currentCameraBlock.maxCameraPitchAngle && targetRotation.eulerAngles.x > 180)
            {
                targetRotation *= Quaternion.Euler(new Vector3(360 - currentCameraBlock.maxCameraPitchAngle - targetRotation.eulerAngles.x, 0, 0));
            }
            else if (targetRotation.eulerAngles.x > currentCameraBlock.maxCameraPitchAngle && targetRotation.eulerAngles.x < 180)
            {
               targetRotation *= Quaternion.Euler(new Vector3(currentCameraBlock.maxCameraPitchAngle - targetRotation.eulerAngles.x, 0, 0));
            }

            targetRotation *= Quaternion.Euler(0, 0, -targetRotation.eulerAngles.z);
            GameManager.instance.RotateCamera(targetRotation);
            GameManager.instance.MoveCamera(newPosition);
        }
        else
        {
            GameManager.instance.RotateCamera(newRotate);
            GameManager.instance.MoveCamera(newPosition);
        }
    }

    public void Zoom(bool b)
    {
        if (b) offset = zoomOffset;
        else offset = normalOffset;
    }

    public void SetNewZ(float newZ)
    {
        zStartPosition = newZ;
    }

    public void SetNewOffset(float newOffset)
    {
        offset = newOffset;
    }

    public void DestroyTarget()
    {
        //print("destroy target camera");
        GameObject g = new GameObject();
        g.transform.position = target.position;
        target = g.transform;
    }
}