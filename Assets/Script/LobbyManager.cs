using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour{

    public CanvasGroup cGroup;

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
    private Vector3 initTitlePos;
    private Vector3 initBtStartPos;
    private Vector3 initBtStagePos;
    private Vector3 initBtSettingPos;
    private Vector3 initBtModePos;

    //
    private Sequence _seqHideAll;
    private Sequence _used;

    //
    private bool _inited = false;

    public void Initialize() {
        if (_inited) {
            //
            title.localPosition = initTitlePos;
            rectStart.localPosition = initBtStartPos;
            rectStage.localPosition = initBtStagePos;
            rectSetting.localPosition = initBtSettingPos;
            rectMode.localPosition = initBtModePos;

            //
            gameObject.SetActive(true);
            cGroup.alpha = 1.0f;

            //
            return;
        }

        // 초기 위치 저장하고
        initTitlePos = title.localPosition;
        initBtStartPos = rectStart.localPosition;
        initBtStagePos = rectStage.localPosition;
        initBtSettingPos = rectSetting.localPosition;
        initBtModePos = rectMode.localPosition;

        // 연출 세팅하고
        if (_seqHideAll == null) {
            _seqHideAll = DOTween.Sequence()
                .OnStart(() => {
                    //
                    title.localPosition = initTitlePos;
                    rectStart.localPosition = initBtStartPos;
                    rectStage.localPosition = initBtStagePos;
                    rectSetting.localPosition = initBtSettingPos;
                    rectMode.localPosition = initBtModePos;

                    //
                    gameObject.SetActive(true);
                    cGroup.alpha = 1.0f;
                })
                .Append(title.DOLocalMoveY(1500f, 2.0f).SetEase(Ease.InOutCirc))
                .Join(rectStart.DOLocalMoveY(-1500f, 2.0f).SetEase(Ease.InOutCirc))
                .Join(rectStage.DOLocalMoveY(-1500f, 2.0f).SetEase(Ease.InOutCirc))
                .Join(rectSetting.DOLocalMoveY(-1500f, 2.0f).SetEase(Ease.InOutCirc))
                .Join(rectMode.DOLocalMoveY(-1500f, 2.0f).SetEase(Ease.InOutCirc))
                .Append(cGroup.DOFade(0.0f, 0.5f))
                .AppendCallback(() => gameObject.SetActive(false));
            _seqHideAll.Pause();
        }

        // TODO: 아직 안쓰임
        btMode.interactable = false;

        // 버튼 세팅
        btStart.onClick.RemoveAllListeners();
        btStart.onClick.AddListener(() => {
            OnStart(() => {
                TilerManager.Get().stageManager.HideAll(() => { });
                TilerManager.Get().editManager.Initialize(200, 200);
            });
        });

        btStage.onClick.RemoveAllListeners();
        btStage.onClick.AddListener(() => {
            OnStage(() => { });
        });

        //
        title.localPosition = initTitlePos;
        rectStart.localPosition = initBtStartPos;
        rectStage.localPosition = initBtStagePos;
        rectSetting.localPosition = initBtSettingPos;
        rectMode.localPosition = initBtModePos;

        //
        gameObject.SetActive(true);
        cGroup.alpha = 1.0f;

        //
        _inited = true;
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
