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
    [SerializeField] float zOffSet = 3;

    Vector3 targetPosition;
    float zStartPosition;

    void Start()
    {
        Vector3 StartPosition = target.position;
        zStartPosition = transform.position.z;
        StartPosition.z = target.position.z+zOffSet;
        StartPosition.y += yOffset;
        transform.position = StartPosition;
    }

    void Update()
    {
        if (!target) return;
        Vector3 cursorPos = target.GetComponent<PlayerController>().GetLookAt();
        Vector3 diffCameraCursor = (cursorPos - transform.position);
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
 