using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilerManager : MonoBehaviour
{
    public EditManager editManager;
    public LobbyManager lobby;
    public StageManager stageManager;

    // http://lonpeach.com/2017/02/04/unity3d-singleton-pattern-example/
    private static TilerManager instance;

    //
    public Transform parentUI;
    public Transform parentContent;

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
        ResourceManager.Get().Initialize();

        //
        editManager.Initialize();
        lobby.Initialize();
        stageManager.Initialize();
    }
}
