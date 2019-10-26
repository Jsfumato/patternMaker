using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilerManager : MonoBehaviour {
    public EditManager editManager;
    public LobbyManager lobby;
    public StageManager stageManager;

    // http://lonpeach.com/2017/02/04/unity3d-singleton-pattern-example/
    private static TilerManager instance;

    //
    public Transform parentUI;
    public Transform parentContent;

    //
    private bool _inited = false;

    public static TilerManager Get() {
        return instance;
    }

    public void Awake() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this) {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //
        AssetBundleManager.Get().LoadAll((bool success) => {
            if (!success) {
                Debug.LogError("AssetBundleManager::LoadAll failed");
                return;
            }

            try {
                //
                ResourceManager.Get().Initialize();

                //
                editManager.Initialize();
                if (PlayerPrefs.GetInt(Constants.KEY.FIRST_VISITOR, 0) > 0) {
                    lobby.Initialize(false);
                    stageManager.Initialize(true);
                } else {
                    lobby.Initialize(true);
                    stageManager.Initialize(false);
                }

                //
                PlayerPrefs.SetInt(Constants.KEY.FIRST_VISITOR, 1);

                //
                _inited = true;
            } catch (Exception e) {
                Application.Quit();
            }
        }, null);
    }

    public void FadeOutAll() {
        if (editManager.isActiveAndEnabled)
            editManager.FadeOutAll(null);
        if (lobby.isActiveAndEnabled)
            lobby.FadeOutAll(null);
        if (stageManager.isActiveAndEnabled)
            stageManager.FadeOutAll(null);
    }

    public void HideAll() {
        editManager.gameObject.SetActive(false);
        lobby.gameObject.SetActive(false);
        stageManager.gameObject.SetActive(false);
    }
}
