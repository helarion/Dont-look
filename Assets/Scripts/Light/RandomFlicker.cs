using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFlicker : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] int indexMax;
    [SerializeField] int indexMin;
    [SerializeField] bool isEnabled=false;
    [SerializeField] string onSound;
    [SerializeField] string offSound;
    [SerializeField] float maxPlayerDistance;

    bool isFlickering = false;
    float playerDistance;
    bool animatorOffOnStart = false;

    private void Start()
    {
        if (!animator.enabled) animatorOffOnStart=true;
    }

    private void Update()
    {
        if (!isEnabled) return;
        playerDistance = (transform.position - GameManager.instance.player.transform.position).magnitude;
        if(playerDistance < maxPlayerDistance)
        {
            animator.enabled = true;
            if (!isFlickering)
            {
                PlayRandomAnim();
            }
        }
        else
        {
            animator.enabled = false;
        }
    }

    public void PlayOnSound()
    {
        AkSoundEngine.PostEvent(onSound, gameObject);
    }

    public void PlayOffSound()
    {
        AkSoundEngine.PostEvent(offSound, gameObject);
    }

    public void PlayRandomAnim()
    {
        //print("playerDistance:"+playerDistance);

        if (playerDistance < maxPlayerDistance)
        {
            isFlickering = true;
            int randomIndex = Random.Range(indexMin, indexMax);
            //print("rand:" + randomIndex);
            if (randomIndex < 0) randomIndex = 0;
            animator.SetInteger("Index", randomIndex);
        }
        else isFlickering = false;
    }

}
