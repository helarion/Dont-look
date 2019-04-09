using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Transform target=null; // target = player, le suivre avec la cam

    [Header("Camera Variables")]
    [SerializeField] float offset=1;
    [SerializeField] float cameraYawAngleMultiplier = 10;
    [SerializeField] float minYCamera = 1.0f;
    
    float zStartPosition;

    void Start()
    {
        zStartPosition = transform.position.z;
    }

    void Update()
    {
        if (!target || GameManager.instance.GetIsPaused()) return;

        Vector3 lookAtPos = GameManager.instance.player.GetLookAt();
        /*Vector3 diffCameraCursor = (cursorPos - transform.position);
        diffCameraCursor.z = zStartPosition;
        diffCameraCursor.y += yOffset;

        diffCameraCursor.x = Mathf.Clamp(diffCameraCursor.x, -offset, offset);
        diffCameraCursor.y = Mathf.Clamp(diffCameraCursor.y, minYCamera, offset);
        Vector3 diffCameraPlayer = transform.position - target.position;

        Vector3 newPosition = target.position + diffCameraPlayer.normalized * offset + diffCameraCursor;*/

        Vector3 newPosition = target.position + Vector3.ClampMagnitude(lookAtPos - target.position, offset);
        if (newPosition.y < minYCamera)
        {
            newPosition.y = minYCamera;
        }
        newPosition.z = zStartPosition;
        
        float cameraYawAngle = GameManager.instance.player.getCursorPosNormalized().x * cameraYawAngleMultiplier;

        Quaternion newRotate = Quaternion.Euler(0, cameraYawAngle, 0);

        CameraBlock currentCameraBlock = GameManager.instance.player.getCameraBlock();
        if (currentCameraBlock != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - lookAtPos);
            if (currentCameraBlock.blockDirection == CameraBlock.BlockDirection.Left)
            {
                if (newPosition.x < currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.x)
                {
                    newPosition.x = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.max.x;
                }

                if (targetRotation.eulerAngles.y < -currentCameraBlock.maxCameraYawAngle)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, -currentCameraBlock.maxCameraYawAngle - targetRotation.eulerAngles.y, 0));
                }
                else if (targetRotation.eulerAngles.y > 0)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, -targetRotation.eulerAngles.y, 0));
                }
            }
            else
            {
                if (newPosition.x > currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.x)
                {
                    newPosition.x = currentCameraBlock.gameObject.GetComponent<BoxCollider>().bounds.min.x;
                }
                
                if (targetRotation.eulerAngles.y < 0)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, -targetRotation.eulerAngles.y, 0));
                }
                else if (targetRotation.eulerAngles.y > currentCameraBlock.maxCameraYawAngle)
                {
                    targetRotation *= Quaternion.Euler(new Vector3(0, currentCameraBlock.maxCameraYawAngle - targetRotation.eulerAngles.y, 0));
                }
            }
            
            
            if (targetRotation.eulerAngles.y < -currentCameraBlock.maxCameraPitchAngle)
            {
                targetRotation *= Quaternion.Euler(new Vector3(-currentCameraBlock.maxCameraPitchAngle - targetRotation.eulerAngles.x, 0, 0));
            }
            else if (targetRotation.eulerAngles.y > currentCameraBlock.maxCameraPitchAngle)
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
}