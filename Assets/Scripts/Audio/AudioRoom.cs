using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRoom : MonoBehaviour
{
    Collider cl;
    bool firstPost = false;
    [SerializeField] AudioSas[] entries;
    [SerializeField] string playEvent;
    [SerializeField] float postOffset = 1;
    [SerializeField] int id;
    float rtpcVolumeFilter;
    float rtpcPan;

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
            if (entry.getIsOccupied())
            {
                if (!firstPost)
                {
                    firstPost = true;
                    AkSoundEngine.PostEvent(playEvent, GameManager.instance.player.gameObject);
                }
                if (entry.direction == AudioSas.Direction.Left)
                {
                    float rate = (GameManager.instance.player.transform.position.x - entry.getCollider().bounds.min.x - postOffset) / (entry.getCollider().bounds.max.x - entry.getCollider().bounds.min.x);
                    rtpcVolumeFilter = Mathf.Clamp(25 * rate, 0, 100);
                    rtpcPan = Mathf.Clamp(25 * rate, 0, 100);
                }
                else if (entry.direction == AudioSas.Direction.Right)
                {
                    float rate = (GameManager.instance.player.transform.position.x - entry.getCollider().bounds.min.x + postOffset) / (entry.getCollider().bounds.max.x - entry.getCollider().bounds.min.x);
                    rtpcVolumeFilter = Mathf.Clamp(25 * (1 - rate), 0, 100);
                    rtpcPan = Mathf.Clamp(100 - 25 * (1 - rate), 0, 100);
                }
                else if (entry.direction == AudioSas.Direction.Up)
                {
                    float rate = (GameManager.instance.player.transform.position.y - entry.getCollider().bounds.min.y - postOffset) / (entry.getCollider().bounds.max.y - entry.getCollider().bounds.min.y);
                    rtpcVolumeFilter = Mathf.Clamp(25 * rate, 0, 100);
                    rtpcPan = Mathf.Clamp(50, 0, 100);
                }
                else if (entry.direction == AudioSas.Direction.Down)
                {
                    float rate = (GameManager.instance.player.transform.position.y - entry.getCollider().bounds.min.y + postOffset) / (entry.getCollider().bounds.max.y - entry.getCollider().bounds.min.y);
                    rtpcVolumeFilter = Mathf.Clamp(25 * (1 - rate), 0, 100);
                    rtpcPan = Mathf.Clamp(50, 0, 100);
                }
                AkSoundEngine.SetRTPCValue("position_relative_volume_"+id, rtpcVolumeFilter);
                AkSoundEngine.SetRTPCValue("position_gd_"+id, rtpcPan);
                //print("Position GD:" + rtpcPan+" de l'id:"+id);
            }
        }
    }
}
