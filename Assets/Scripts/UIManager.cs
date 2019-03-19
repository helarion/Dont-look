using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image fadeImg = null;
    [SerializeField] float fadeTime=0.1f;

    public bool isFading = false;

    public static UIManager instance = null;

    void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    IEnumerator FadeImage(bool inout)
    {
        // fade from transparent to opaque
        if(inout)
        {
            isFading = true;
            // loop over 1 second
            for (float i = 0; i <= 1; i += Time.deltaTime*fadeTime)
            {
                // set color with i as alpha
                fadeImg.color = new Color(0, 0, 0, i);
                yield return null;
            }
            isFading = false;
        }
        // fade from opaque to transparent
        else
        {
            yield return new WaitForSeconds(1);
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime * fadeTime)
            {
                // set color with i as alpha
                fadeImg.color = new Color(0, 0, 0, i);
                yield return null;
            }
            //isFading = false;
        }
    }

    public void FadeIn(bool inout)
    {
        StartCoroutine("FadeImage",inout);
    }
}
