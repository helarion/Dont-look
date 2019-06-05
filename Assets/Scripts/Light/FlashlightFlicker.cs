using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightFlicker : MonoBehaviour
{
    [SerializeField] string flashlightOnSound;
    [SerializeField] string flashlightOffSound;

    public void PlayOnSound()
    {
        //AkSoundEngine.PostEvent(flashlightOnSound, gameObject);
    }

    public void PlayOffSound()
    {
        //AkSoundEngine.PostEvent(flashlightOffSound, gameObject);
    }
}
