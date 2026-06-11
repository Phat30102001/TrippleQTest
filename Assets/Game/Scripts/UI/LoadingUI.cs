using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PeopleFlow.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : UIBase
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI  loadingTextImage;
    // [SerializeField] private GuiSliceContainer _guiSliceContainer;
    [SerializeField] private float _loadingSliceTimeGap=0.1f;
    private Action _onFinished;
    private Action<Action> _onPreLoad;
    private Action<Action> _onInitComplete;

    public override void Init(params object[] dependencies)
    {

    }


    public override void SetData(params object[] data)
    {

        _onInitComplete = data[0] as Action<Action>;
        _onPreLoad = data[1] as Action<Action>;
        _onFinished = data[2] as Action;
        StartCoroutine(CRLoadingGame());
        StartCoroutine(LoadingAnimation());

    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        loadingTextImage.DOKill();
        gameObject.SetActive(false);
        fillImage.fillAmount = 0f;
        StopCoroutine(LoadingAnimation());
    }

    IEnumerator CRLoadingGame()
    {
        var isInitComplete = false;
        var isPreLoaded = false;
        fillImage.fillAmount = 0f;
        DOVirtual.Float(fillImage.fillAmount, fillImage.fillAmount + 0.25f, 0.5f,
            value => { fillImage.fillAmount = value; });
        yield return new WaitForSeconds(0.5f);
        DOVirtual.Float(fillImage.fillAmount, fillImage.fillAmount + 0.25f, 0.5f,
            value => { fillImage.fillAmount = value; });
        yield return new WaitForSeconds(0.5f);
        _onInitComplete?.Invoke(() => { isInitComplete = true; });
        yield return new WaitUntil(() => isInitComplete);
        DOVirtual.Float(fillImage.fillAmount, fillImage.fillAmount + 0.25f, 0.2f,
            value => { fillImage.fillAmount = value; });
        yield return new WaitForSeconds(0.2f);
        _onPreLoad?.Invoke(() => { isPreLoaded = true; });
        yield return new WaitUntil(() => isPreLoaded);
        DOVirtual.Float(fillImage.fillAmount, fillImage.fillAmount + 0.25f, 0.4f,
            value => { fillImage.fillAmount = value; });
        yield return new WaitForSeconds(0.4f);
        DOVirtual.Float(fillImage.fillAmount, fillImage.fillAmount + 0.25f, 0.4f,
            value => { fillImage.fillAmount = value; });
        yield return new WaitForSeconds(0.4f);
        _onFinished?.Invoke();
        Hide();
    }

    private IEnumerator LoadingAnimation()
    {
        List<int> list = new List<int>();
        int pos = 0;
        while (gameObject.activeSelf)
        {
            list.Add(pos);
            pos++;
            // _guiSliceContainer.SetData(list);
            yield return new WaitForSeconds(_loadingSliceTimeGap);
            if (pos >= 6)
            {
                pos = 0;
                list.Clear();
            }
        }
    }
}
