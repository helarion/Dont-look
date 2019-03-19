using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Transform target=null;
    [SerializeField] float cameraSpeed=1;
    [SerializeField] float offset=1;
    [SerializeField] float minYCamera = 1;
    [SerializeField] float yOffset = 1;

    Vector3 targetPosition;
    float zStartPosition;

    void Start()
    {
        zStartPosition = transform.position.z;
        Vector3 StartPosition = target.position;
        StartPosition.z = zStartPosition;
        StartPosition.y += yOffset;
        transform.position = StartPosition;

        //cameraSpeed = GameManager.instance.player.getLightSpeed();
    }

    void Update()
    {
        if (!target) return;
        Vector3 cursorPos = target.GetComponent<PlayerController>().GetCursorPos();
        Vector3 diffCameraCursor = cursorPos - transform.position;
        diffCameraCursor.z = zStartPosition;
        diffCameraCursor.y += yOffset;

        diffCameraCursor.x = Mathf.Clamp(diffCameraCursor.x, -offset, offset);
        diffCameraCursor.y = Mathf.Clamp(diffCameraCursor.y, minYCamera, offset);
        Vector3 diffCameraPlayer = transform.position - target.position;

        Vector3 newPosition = target.position + diffCameraPlayer.normalized * offset + diffCameraCursor;
        newPosition.z = zStartPosition;

        transform.position = Vector3.Lerp(transform.position, newPosition, cameraSpeed);
    }
}
 