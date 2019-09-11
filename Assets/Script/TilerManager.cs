using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilerManager : MonoBehaviour
{
    public EditTile editManager;
    public LobbyManager lobby;
    public StageManager stageManager;

    public void Awake() {
        //
        ResourceManager.Get().Initialize();

        //
        editManager.Initialize();
        lobby.Initialize();
        stageManager.Initialize();
    }
}
