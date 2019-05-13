using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occlusion_v2 : MonoBehaviour
{
    public GameObject listener;
    public float time_start = 0.5f;
    public float time_end = 1.0f; 
    public float current_occlusion = 0.0f;
    public float max_occlusion = 100.0f;
    public LayerMask layerMaskOcclusion;
    private GameObject[] soundEmitter = null;

   // public AK.Wwise.Event wwiseEvent;

    // Use this for initialization
    void Start () {

        //AkSoundEngine.PostEvent(wwiseEvent.Id, gameObject);
        //listener = GameObject.FindGameObjectWithTag("Player");
        layerMaskOcclusion = LayerMask.GetMask("Walls");
    }

	// Update is called once per frame
	void Update ()
	{
        // Init
		RaycastHit hitInfo;
		float playerDistance;

        // Calc
        Vector3 current_position = transform.position;
        current_position.y += 1f;
		Vector3 raycastDir = listener.transform.position - transform.position;
		playerDistance = Vector3.Distance(transform.position, listener.transform.position);

        // Check if something occlude
        if (Physics.Raycast(new Ray(transform.position, raycastDir), out hitInfo, playerDistance, layerMaskOcclusion))
        {
            // Hit wall
            Debug.DrawRay(current_position, raycastDir, Color.red);
            if (current_occlusion < 100.0f)
                current_occlusion += max_occlusion / (time_end / Time.deltaTime);
        }
        else
        {
            // Don't hit wall
            Debug.DrawRay(current_position, raycastDir, Color.blue);
            if (current_occlusion > 0.0f)
                current_occlusion -= max_occlusion / (time_start / Time.deltaTime);
        }

        // Occlusion
        current_occlusion = Mathf.Clamp(current_occlusion, 0, max_occlusion);
        //print(current_occlusion);

        // Wwise
        AkSoundEngine.SetObjectObstructionAndOcclusion(gameObject, listener, 0.0f, current_occlusion / 100f);
    }
}
