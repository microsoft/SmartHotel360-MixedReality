using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HololensScrollInput : MonoBehaviour
{
    public ScrollRect ScrollView;

    public void OnScrollDown()
    {
        if (ScrollView == null)
            return;

        var perItem = 1f / ScrollView.content.childCount;

        StartCoroutine(Scroll(-(perItem * 2)));
    }

    public void OnScrollUp()
    {
        if (ScrollView == null)
            return;

        var perItem = 1f / ScrollView.content.childCount;

        StartCoroutine(Scroll(perItem * 2));
    }

    private IEnumerator Scroll(float amount)
    {
        Canvas.ForceUpdateCanvases();

        yield return new WaitForEndOfFrame();

        ScrollView.verticalNormalizedPosition += amount;

        Canvas.ForceUpdateCanvases();
    }
}
