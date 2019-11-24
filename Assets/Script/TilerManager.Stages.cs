using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class TilerManager {

    [Header("Stage UI")]
    public CanvasGroup cGroupStages;
    public GridLayoutGroup table;
    public GameObject cell;

    //
    private Sequence _seqHideAllStages;
    private Sequence _usedForStages;

    public void RefreshStages() {

        //
        if (cell.activeSelf)
            cell.SetActive(false);

        // stage 정보 세팅
        foreach (ResourceStage resStage in ResourceManager.Get().GetStages()) {
            var cloned = GameObject.Instantiate(cell);
            var stageInfo = cloned.GetComponent<StageCell>();
            if (stageInfo == null)
                stageInfo = cloned.AddComponent<StageCell>();

            // 정보 입력
            stageInfo.Initialize(resStage);

            // 터치하면 퍼즐로 이동
            var btTouched = cloned.GetComponent<Button>();
            if (btTouched == null)
                btTouched = cloned.AddComponent<Button>();
            btTouched.onClick.RemoveAllListeners();
            btTouched.onClick.AddListener(() => {
                FadeOutAllStages(() => {
                    RefreshPallete(resStage);
                });
            });

            // UI에 표기
            cloned.transform.SetParent(table.transform);
            cloned.SetActive(true);
        }

        // 연출 세팅하고
        _seqHideAllStages = DOTween.Sequence()
            .OnStart(() => {
                gameObject.SetActive(true);
                cGroupStages.alpha = 1.0f;
            })
            .Append(cGroupStages.DOFade(0.0f, 0.5f))
            .AppendCallback(() => gameObject.SetActive(false));
        _seqHideAllStages.Pause();

        //
        cGroupStages.alpha = 1f;
    }

    public void FadeInAllStages(TweenCallback callback) {
        if (_usedForStages != null)
            _usedForStages.Kill();

        //
        gameObject.SetActive(true);
        cGroupStages.alpha = 1.0f;
    }

    public void FadeOutAllStages(TweenCallback callback) {
        if (_usedForStages != null)
            _usedForStages.Kill();

        //
        _usedForStages = _seqHideAllStages.OnComplete(callback);
        _usedForStages.Play();
    }

    public void HideAllStages(TweenCallback callback) {
        if (_usedForStages != null)
            _usedForStages.Kill();

        //
        gameObject.SetActive(false);
        if (callback != null)
            callback();
    }
}
