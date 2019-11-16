using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TilerManager : MonoBehaviour {
    //public EditManager editManager;
    
    // http://lonpeach.com/2017/02/04/unity3d-singleton-pattern-example/
    private static TilerManager instance;

    [Header("Main UI")]
    public Transform parentUI;
    public Transform parentContent;

    public static TilerManager Get() {
        if (instance == null)
            instance = FindObjectOfType<TilerManager>();

        //
        if (instance == null) { 
            var go = new GameObject();
            instance = go.AddComponent<TilerManager>();
            DontDestroyOnLoad(go);
        }

        //
        return instance;
    }

    public void Awake() {
        // 명시적 초기화
        Get().Initialize();
    }

    public void Initialize() {
        // 싱글톤 세팅은 Get에서

        //
        AssetBundleManager.Get().LoadAll((bool success) => {
            if (!success) {
                Debug.LogError("AssetBundleManager::LoadAll failed");
                return;
            }

            try {
                //
                ResourceManager.Get().Initialize();

                // 첫 실행인지 확인, 첫 실행이 아니라면 바로 stage 보여줌
                if (PlayerPrefs.GetInt(Constants.KEY.FIRST_VISITOR, 0) > 0) {
                    //lobby.Initialize(false);
                    RefreshStages();
                // 첫 실행이라면 lobby 먼저 보여줌
                } else {
                    //lobby.Initialize(true);
                    RefreshStages();
                }
                PlayerPrefs.SetInt(Constants.KEY.FIRST_VISITOR, 1);

                //
                RefreshPallete();
            } catch (Exception e) {
                Application.Quit();
            }
        }, null);
    }

    public void FadeOutAll() {
        FadeOutAllStages(null);
        FadeOutAllPalette(null);
    }

    public void HideAll() {
        HideAllStages(null);
        HideAllPalette(null);
    }
}
