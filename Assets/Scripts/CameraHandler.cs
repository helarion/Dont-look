using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{

    [SerializeField] Transform target=null;
    [SerializeField] float cameraSpeed=1;
    [SerializeField] float offset=1;
    [SerializeField] float cursorLookBias=1;
    [SerializeField] bool moveCamera = true;

    Vector3 targetPosition;

    void Start()
    {
        transform.position = target.position;
    }

    void Update()
    {
        if (target && moveCamera)
        {
            Vector3 cursorPos = target.GetComponent<PlayerController>().getCursorPos();
            Vector3 diffCameraCursor = cursorPos - transform.position;
            diffCameraCursor.z = 1.516556f;
            if (diffCameraCursor.y < 0.5f)
            {
                diffCameraCursor.y = 0.5f;
            }
            transform.position = diffCameraCursor;
            Vector3 diffCameraPlayer = transform.position - target.position;
            if (diffCameraPlayer.magnitude > 2f)
            {
                Vector3 newPosition = target.position + diffCameraPlayer.normalized * 2f;
                if (newPosition.y < 0.5f)
                {
                    newPosition.y = 0.5f;
                }
                newPosition.z = 1.516556f;
                transform.position = newPosition;
            }
            else
            {
                transform.position = diffCameraCursor;
            }
            return;
        }
    }
    /*
        if(target && moveCamera)
        {
            Vector3 newPos = transform.position;
            newPos.x = target.position.x;
            //newPos.y = target.position.y;

            if (Input.GetAxis("Horizontal")<0)
            {
                // left
                newPos.x -= offset;
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                // right
                newPos.x += offset;
            }
            /*
            if (Input.GetAxis("Vertical") <0)
            {
                //lower
                newPos.y -= offset;
            }

            if (Input.GetAxis("Vertical") > 0)
            {
                // upper
                newPos.y += offset;
            }*/

            /*transform.position = Vector3.Lerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
        }
        
       Vector3 v3 = Input.mousePosition;
        //v3.z = 7f;
        v3 = GameManager.instance.mainCamera.ScreenToWorldPoint(v3);
        transform.position = Vector3.Lerp(transform.position, v3, cursorLookBias);
     }*/
}