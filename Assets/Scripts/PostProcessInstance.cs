using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessInstance : MonoBehaviour
{
    [HideInInspector] public static PostProcessInstance instance = null;
    public PostProcessVolume volume;

    [HideInInspector] public ColorGrading colorGrading;
    [HideInInspector] public Grain grain;
    [HideInInspector] public Vignette vignette;
    [HideInInspector] public Bloom bloom;

    private void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        PostProcessInstance.instance.volume.profile.TryGetSettings(out colorGrading);
        PostProcessInstance.instance.volume.profile.TryGetSettings(out grain);
        PostProcessInstance.instance.volume.profile.TryGetSettings(out vignette);
        PostProcessInstance.instance.volume.profile.TryGetSettings(out bloom);
        colorGrading.gamma.value = new Vector4(0, 0, 0, 0);
    }

    private void Start()
    {

        
    }
}
