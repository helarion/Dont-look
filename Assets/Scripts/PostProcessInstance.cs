using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessInstance : MonoBehaviour
{
    [HideInInspector] public static PostProcessInstance instance = null;
    public PostProcessVolume volume;

    private void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
