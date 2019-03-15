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

    void Update()
    {
        if (target && moveCamera)
        {
            Vector3 cursorPos = target.GetComponent<PlayerController>().getCursorPos();
            Vector3 diffCameraCursor = cursorPos - transform.position;
            transform.position = new Vector3 (diffCameraCursor.x, diffCameraCursor.y, 1.516556f);
            Vector3 diffCameraPlayer = transform.position - target.position;
            if (diffCameraPlayer.magnitude > 1.5f)
            {
                Vector3 newPosition = target.position + diffCameraPlayer.normalized * 1.5f;
                newPosition.z = 1.516556f;
                transform.position = newPosition;
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