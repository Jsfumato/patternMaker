using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour{

    [Header("Buttons")]
    public Button btStart;
    public Button btStage;
    public Button btSetting;
    public Button btMode;

    [Header("Transform")]
    public RectTransform title;
    public RectTransform rectStart;
    public RectTransform rectStage;
    public RectTransform rectSetting;
    public RectTransform rectMode;

    //
    private Sequence _seqHideAll;
    private Sequence _used;

    public void Initialize() {
        //
        _seqHideAll = DOTween.Sequence()
            .Append(title.DOLocalMoveY(500f, 2.0f).SetEase(Ease.InOutCirc))
            .Join(rectStart.DOLocalMoveY(-500f, 2.0f).SetEase(Ease.InOutCirc))
            .Join(rectStage.DOLocalMoveY(-500f, 2.0f).SetEase(Ease.InOutCirc))
            .Join(rectSetting.DOLocalMoveY(-500f, 2.0f).SetEase(Ease.InOutCirc))
            .Join(rectMode.DOLocalMoveY(-500f, 2.0f).SetEase(Ease.InOutCirc))
            .SetDelay(3f);
        _seqHideAll.Pause();

        // TODO: 아직 안쓰임
        btMode.interactable = false;

        // 버튼 세팅
        btStart.onClick.RemoveAllListeners();
        btStart.onClick.AddListener(() => {
            OnStart(() => { });
        });
    }

    // 애니메이션 지정
    public void HideAll(TweenCallback callback) {
        if (_used != null)
            _used.Kill();

        //
        _used = _seqHideAll.OnComplete(callback);
        _used.Play();
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
