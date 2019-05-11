using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessInstance : MonoBehaviour
{
    [HideInInspector] public static PostProcessInstance instance = null;
    public PostProcessVolume volume;

    ColorGrading colorGrading;

    private void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PostProcessInstance.instance.volume.profile.TryGetSettings(out colorGrading);
        colorGrading.gamma.value = new Vector4(0, 0, 0, 0);
    }
}
