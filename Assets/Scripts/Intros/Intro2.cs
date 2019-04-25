using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro2 : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float waitBefore=0.5f;
    [SerializeField] private Image fadeImg;
    [SerializeField] int scene=2;

    private void Start()
    {
        if (scene == -1)
        {
            Destroy(GameManager.instance.gameObject);
            Destroy(UIManager.instance.gameObject);
        }
        StartCoroutine("FadeOutCoroutine");
    }

    IEnumerator StartCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        GameManager.instance.SetIsPaused(false);
    }

    IEnumerator FadeOutCoroutine()
    {
        Color savedColor = fadeImg.color;
        savedColor.a = 1;
        fadeImg.color = savedColor;
        for (float i = 1; i > 0; i -= Time.deltaTime/2)
        {
            // set color with i as alpha
            savedColor.a = i;
            fadeImg.color = savedColor;
            yield return new WaitForEndOfFrame();
        }
        savedColor.a = 0;
        fadeImg.color = savedColor;
        yield return new WaitForSeconds(waitBefore);
        StartCoroutine("FadeInCoroutine");
    }

    IEnumerator FadeInCoroutine()
    {
        Color savedColor = fadeImg.color;
        savedColor.a = 0;
        fadeImg.color = savedColor;

        for (float i = 0; i < fadeDuration; i += Time.deltaTime)
        {
            // set color with i as alpha
            savedColor.a = i;
            fadeImg.color = savedColor;
            yield return new WaitForEndOfFrame();
        }
        savedColor.a = 1;
        fadeImg.color = savedColor;
        yield return new WaitForSeconds(1);

        if(scene!=-1)SceneManager.LoadScene(scene);
    }
}
