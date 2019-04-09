using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRoom : MonoBehaviour
{
    Collider cl;
    bool firstPost = false;
    [SerializeField] AudioSas[] entries;
    [SerializeField] AK.Wwise.Event playEvent;
    public float rtpcVolumeFilter;
    public float rtpcPan;

    // Start is called before the first frame update
    void Start()
    {
        cl = GetComponent<Collider>();
        rtpcVolumeFilter = 0;
        rtpcPan = 50;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (AudioSas entry in entries)
        {
            if (entry.occupied)
            {
                if (!firstPost)
                {
                    firstPost = true;
                    AkSoundEngine.PostEvent(playEvent.Id, gameObject);
                }
                if (entry.direction == AudioSas.Direction.Left)
                {
                    float rate = 1 - (GameManager.instance.player.transform.position.x - entry.getCollider().bounds.min.x) / (entry.getCollider().bounds.max.x - entry.getCollider().bounds.min.x);
                    rtpcVolumeFilter = Mathf.Clamp(25 * rate, 0, 100);
                    rtpcPan = Mathf.Clamp(25 * rate, 0, 100);
                    AkSoundEngine.SetRTPCValue("position_relative_volume", rtpcVolumeFilter);
                    AkSoundEngine.SetRTPCValue("position_gd", rtpcPan);
                }
                else if (entry.direction == AudioSas.Direction.Right)
                {
                    float rate = 1 - (GameManager.instance.player.transform.position.x - entry.getCollider().bounds.min.x) / (entry.getCollider().bounds.max.x - entry.getCollider().bounds.min.x);
                    rtpcVolumeFilter = Mathf.Clamp(25 * (1 - rate), 0, 100);
                    rtpcPan = Mathf.Clamp(100 - 25 * (1 - rate), 0, 100);
                    AkSoundEngine.SetRTPCValue("position_relative_volume", rtpcVolumeFilter);
                    AkSoundEngine.SetRTPCValue("position_gd", rtpcPan);
                }
                else if (entry.direction == AudioSas.Direction.Up)
                {
                    float rate = 1 - (GameManager.instance.player.transform.position.y - entry.getCollider().bounds.min.y) / (entry.getCollider().bounds.max.y - entry.getCollider().bounds.min.y);
                    rtpcVolumeFilter = Mathf.Clamp(25 * (1 - rate), 0, 100);
                    rtpcPan = Mathf.Clamp(50, 0, 100);
                    AkSoundEngine.SetRTPCValue("position_relative_volume", rtpcVolumeFilter);
                    AkSoundEngine.SetRTPCValue("position_gd", rtpcPan);
                }
                else if (entry.direction == AudioSas.Direction.Down)
                {
                    float rate = 1 - (GameManager.instance.player.transform.position.y - entry.getCollider().bounds.min.y) / (entry.getCollider().bounds.max.y - entry.getCollider().bounds.min.y);
                    rtpcVolumeFilter = Mathf.Clamp(25 * rate, 0, 100);
                    rtpcPan = Mathf.Clamp(50, 0, 100);
                    AkSoundEngine.SetRTPCValue("position_relative_volume", rtpcVolumeFilter);
                    AkSoundEngine.SetRTPCValue("position_gd", rtpcPan);
                }
            }
        }
    }
}
