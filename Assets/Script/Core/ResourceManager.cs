using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : MonoBehaviour {

    private IDictionary<long, JOb> stages = new Dictionary<long, ResourceStage>();

    private static ResourceManager singleton = new ResourceManager();
    private bool inited = false;

    const string PATCH_FOLDER_NAME = "Patches";
    const string DIR_PATCHES = "Assets/" + PATCH_FOLDER_NAME + "/";


    public void Reload() {
        if (singleton == null)
            singleton = new ResourceManager();

        //
        if (!singleton.inited) {
            singleton.Initialize();
        }

        ////
        //System.GC.Collect();
    }

    public void Initialize() {

        //
        singleton.inited = true;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        Initialize();
    }
}
