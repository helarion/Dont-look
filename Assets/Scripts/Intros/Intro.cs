using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private SpriteRenderer cnam =null;
    [SerializeField] private SpriteRenderer magelis =null;
    [SerializeField] private SpriteRenderer poitiers =null;
    [SerializeField] private Transform scene =null;
    [SerializeField] private Canvas logos =null;
    [SerializeField] private Image fadeImg;

    private Color cnamColor;
    private Color magelisColor;
    private Color poitiersColor;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        cnamColor = cnam.color;
        magelisColor = magelis.color;
        poitiersColor = poitiers.color;
        cnamColor.a = 0;
        magelisColor.a = 0;
        poitiersColor.a = 0;

        cnam.color = cnamColor;
        magelis.color = magelisColor;
        poitiers.color = poitiersColor;

        StartCoroutine("FadeInImage");
    }

    private void Update()
    {
        Vector3 newPos = logos.transform.position;
        newPos.z -= Time.deltaTime * zoomSpeed;
        logos.transform.position = newPos;
        scene.position = Vector3.Lerp(scene.position, scene.position + Vector3.back, 0.5f);
        if (Input.GetKeyDown(KeyCode.F1)) SceneManager.LoadScene(3);
    }

    private IEnumerator FadeInImage()
    {
        Color fadeColor = fadeImg.color;
        for (float i = 1; i > 0; i-= Time.deltaTime * 2)
        {
            fadeColor.a = i;
            fadeImg.color = fadeColor;
            yield return new WaitForEndOfFrame();
        }
        fadeColor.a = 0;
        fadeImg.color = fadeColor;
        for (float i = 0; i < 1; i += Time.deltaTime * fadeTime)
        {
            magelisColor.a = i;
            poitiersColor.a = i;
            magelis.color = magelisColor;
            poitiers.color = poitiersColor;
            yield return new WaitForEndOfFrame();
        }

        magelisColor.a = 1;
        poitiersColor.a = 1;
        magelis.color = magelisColor;
        poitiers.color = poitiersColor;

        yield return new WaitForSeconds(1);
        for (float i = 0; i < 1; i += Time.deltaTime * fadeTime)
        {
            cnamColor.a = i;
            cnam.color = cnamColor;

            yield return null;
        }

        cnamColor.a = 1;
        cnam.color = cnamColor;

        yield return new WaitForSeconds(1);
        StartCoroutine("FadeOutImage");
    }

    private IEnumerator FadeOutImage()
    {
        for (float i = 1; i > 0; i -= Time.deltaTime * fadeTime)
        {
            magelisColor.a = i;
            poitiersColor.a = i;
            cnamColor.a = i;
            magelis.color = magelisColor;
            poitiers.color = poitiersColor;
            cnam.color = cnamColor;

            yield return null;
        }
        magelisColor.a = 0;
        poitiersColor.a = 0;
        cnamColor.a = 0;
        magelis.color = magelisColor;
        poitiers.color = poitiersColor;
        cnam.color = cnamColor;

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }
}
