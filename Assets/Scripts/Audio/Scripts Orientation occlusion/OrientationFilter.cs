// using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrientationFilter : MonoBehaviour
{
    #region Attributes
    [SerializeField]
    private AK.Wwise.RTPC orientationRTPC;
    [SerializeField]
    [Range(0, 360)]
    private float angleFront = 90;
    [SerializeField]
    [Range(0, 360)]
    private float angleVariation = 90;
    [SerializeField]
    private float angleBehind = 180;
    public LayerMask layerMaskOcclusion;
    private Transform player;
    private GameObject[] soundEmitter = null;

    public bool showDebug = false;

private void OnValidate()
    {
        if (angleVariation + angleFront > 360)
            angleVariation = 360 - angleFront;
        angleBehind = 360 - (angleFront + angleVariation);
		
#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }
    #endregion

    #region Methods
    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        soundEmitter = GameObject.FindGameObjectsWithTag("SoundObjs");

            for (int i = 0; i < soundEmitter.Length; i++)
            {
                soundEmitter[i].AddComponent<Occlusion_v2>();
            }

}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < soundEmitter.Length; i++)
        {
        this.jesaispas(soundEmitter[i]);
        }
    }

    private void jesaispas(GameObject currentGameObject)
    {
        Vector3 playerForward = new Vector3(player.forward.x, 0, player.forward.z);
        Vector3 playerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 playerToObject = new Vector3(currentGameObject.transform.position.x, 0, currentGameObject.transform.position.z) - playerPos;
        float anglePlayerObject = Vector3.Angle(playerForward, playerToObject);

        if (anglePlayerObject <= angleFront / 2)
        {
            // Player looking object
            AkSoundEngine.SetRTPCValue(orientationRTPC.Id, 100, currentGameObject);
        }
        else
        {
              if (!(anglePlayerObject - (angleFront / 2) <= angleVariation / 2))
            {
                // Object is on the sides of the player
                float rtpcValue = ((anglePlayerObject - (angleFront / 2)) / (angleVariation / 2)) * 100;
                AkSoundEngine.SetRTPCValue(orientationRTPC.Id, rtpcValue, currentGameObject);
            }
            else
            {
                // Player not looking object
                AkSoundEngine.SetRTPCValue(orientationRTPC.Id, 0, currentGameObject);
             }
        }
    } 

    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        Vector3 playerForward = new Vector3(player.forward.x, 0, player.forward.z);
        Vector3 playerPos = new Vector3(player.position.x, player.position.y, player.position.z);
        for (int i = 0; i < soundEmitter.Length; i++)
        {
            drawGizmosEmitter(soundEmitter[i], Vector3.Angle(player.forward, soundEmitter[i].transform.position - player.position));
            Gizmos.DrawWireSphere(soundEmitter[i].transform.position, 2);
        }


        Color defaultColor = Gizmos.color;
        Gizmos.color = Color.cyan;

        for (int i = -90; i < 90; i++)
        {
            RaycastHit hitInfo;
            float rayDegrees = i * 2;
            Quaternion rayRotation = Quaternion.AngleAxis(rayDegrees, Vector3.up);
            Vector3 rayDirection = rayRotation * playerForward;
            Vector3 raycastDir = rayDirection * 5;
            Color color;

            color = ((Mathf.Abs(rayDegrees) <= angleFront / 2) ? Color.green : ((Mathf.Abs(rayDegrees) - (angleFront / 2) <= angleVariation / 2) ? Color.yellow : Color.red)) * 0.8f;
            Debug.DrawRay(playerPos, raycastDir, color);
        }
    }

    private void drawGizmosEmitter(GameObject emGO, float angle)
    {
        for (int i = -90; i < 90; i++)
        {
            RaycastHit hitInfo;
            float rayDegrees = i * 2;
            Quaternion rayRotation = Quaternion.AngleAxis(rayDegrees, Vector3.up);
            Vector3 rayDirection = rayRotation * Vector3.forward;
            Vector3 raycastDir = rayDirection * 4;
            Color color;

            color = ((angle <= angleFront / 2) ? Color.green : ((angle - (angleFront / 2) <= angleVariation / 2) ? Color.yellow : Color.red)) * 0.8f;
            Debug.DrawRay(emGO.transform.position, raycastDir, color);
        }

    }
    #endregion
}
