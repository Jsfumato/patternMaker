using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour{

    public RectTransform title;
    public RectTransform btStart;
    public RectTransform btStage;
    public RectTransform btSetting;
    public RectTransform btMode;

    //
    private Sequence _seqHideAll;

    public void Initialize() {
        //
        _seqHideAll = DOTween.Sequence()
            .Append(title.DOLocalMoveY(500f, 1.0f).SetEase(Ease.InOutCirc))
            .Append(btStart.DOLocalMoveY(-500f, 1.0f).SetEase(Ease.InOutCirc))
            .Append(btStage.DOLocalMoveY(-500f, 1.0f).SetEase(Ease.InOutCirc))
            .Append(btSetting.DOLocalMoveY(-500f, 1.0f).SetEase(Ease.InOutCirc))
            .Append(btMode.DOLocalMoveY(-500f, 1.0f).SetEase(Ease.InOutCirc))
            .SetDelay(1.5f);
    }

    // 애니메이션 지정
    public void HideAll(TweenCallback callback) {
        var modified = _seqHideAll.AppendCallback(callback);
        modified.Play();
    }

    // 타일 제작 UI 바로 진입
    public void OnStart(TweenCallback callback) {
        HideAll(callback);
    }

    // stage 리스트 출력
    public void OnStage(TweenCallback callback) {
        HideAll(callback);
    }

    public void OnSetting() {
        
    }
}
