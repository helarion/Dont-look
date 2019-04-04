using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Transform target=null; // target = player, le suivre avec la cam

    [Header("Camera Variables")]
    [SerializeField] float offset=1;
    [SerializeField] float minYCamera = 1;
    [SerializeField] float yOffset = 1;
    [SerializeField] float cameraYawAngleMultiplier = 10;
    
    float zStartPosition;

    void Start()
    {
        zStartPosition = transform.position.z;
    }

    void Update()
    {
        if (!target || GameManager.instance.GetIsPaused()) return;
        Vector3 cursorPos = GameManager.instance.player.GetLookAt();
        /*Vector3 diffCameraCursor = (cursorPos - transform.position);
        diffCameraCursor.z = zStartPosition;
        diffCameraCursor.y += yOffset;

        diffCameraCursor.x = Mathf.Clamp(diffCameraCursor.x, -offset, offset);
        diffCameraCursor.y = Mathf.Clamp(diffCameraCursor.y, minYCamera, offset);
        Vector3 diffCameraPlayer = transform.position - target.position;

        Vector3 newPosition = target.position + diffCameraPlayer.normalized * offset + diffCameraCursor;*/

        Vector3 newPosition = (target.position + cursorPos) / 2.0f;
        newPosition.z = target.position.z;
        newPosition = target.position + Vector3.ClampMagnitude((cursorPos - target.position), offset);
        newPosition.z = target.position.z;
        
        float cameraYawAngle = -180 + GameManager.instance.player.getCursorPosNormalized().x * cameraYawAngleMultiplier;
        //transform.localRotation = Quaternion.Euler(0, cameraYawAngle, 0);

        Quaternion newRotate = Quaternion.Euler(0, cameraYawAngle, 0);

        newPosition.z = zStartPosition;

        GameManager.instance.RotateCamera(newRotate);
        GameManager.instance.MoveCamera(newPosition);
    }
}
 