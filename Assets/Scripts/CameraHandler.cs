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

    Vector3 targetPosition;
    float zStartPosition;

    void Start()
    {
        zStartPosition = transform.position.z;
    }

    void Update()
    {
        if (!target || GameManager.instance.GetIsPaused()) return;
        Vector3 cursorPos = target.GetComponent<PlayerController>().GetLookAt();
        Vector3 diffCameraCursor = (cursorPos - transform.position);
        diffCameraCursor.z = zStartPosition;
        diffCameraCursor.y += yOffset;

        diffCameraCursor.x = Mathf.Clamp(diffCameraCursor.x, -offset, offset);
        diffCameraCursor.y = Mathf.Clamp(diffCameraCursor.y, minYCamera, offset);
        Vector3 diffCameraPlayer = transform.position - target.position;

        Vector3 newPosition = target.position + diffCameraPlayer.normalized * offset + diffCameraCursor;
        newPosition.z = zStartPosition;

        GameManager.instance.MoveCamera(newPosition);
    }
}
 