using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    [SerializeField] float fadeTime = 0.5f;
    [SerializeField] float zoomTime = 0.1f;
    [SerializeField] SpriteRenderer cnam;
    [SerializeField] SpriteRenderer magelis;
    [SerializeField] SpriteRenderer poitiers;

    Color cnamColor;
    Color magelisColor;
    Color poitiersColor;



    // Start is called before the first frame update
    void Start()
    {
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
        Vector2 cnamSize = cnam.transform.localScale;
        cnamSize.x += Time.deltaTime * zoomTime;
        cnamSize.y += Time.deltaTime * zoomTime;

        Vector2 poitiersSize = poitiers.transform.localScale;
        poitiersSize.x += Time.deltaTime * zoomTime * 0.3f;
        poitiersSize.y += Time.deltaTime * zoomTime * 0.3f;

        Vector2 magelisSize = magelis.transform.localScale;
        magelisSize.x += Time.deltaTime * zoomTime;
        magelisSize.y += Time.deltaTime * zoomTime;

        cnam.transform.localScale = cnamSize;
        poitiers.transform.localScale = poitiersSize;
        magelis.transform.localScale = magelisSize;
    }

    IEnumerator FadeInImage()
    {
        yield return new WaitForSeconds(0.5f);
        for (float i = 0; i < 1; i += Time.deltaTime * fadeTime)
        {
            magelisColor.a = i;
            poitiersColor.a = i;
            magelis.color = magelisColor;
            poitiers.color = poitiersColor;

            yield return null;
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

    IEnumerator FadeOutImage()
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
