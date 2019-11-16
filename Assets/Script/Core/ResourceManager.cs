using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager {

    private static ResourceManager singleton = new ResourceManager();
    private bool inited = false;
    const string PATCH_FOLDER_NAME = "Patches";
    const string DIR_PATCHES = "Assets/" + PATCH_FOLDER_NAME + "/";

    //
    private Dictionary<long, ResourceStage> stages = new Dictionary<long, ResourceStage>();

    //
    public ICollection<ResourceStage> GetStages() {
        return stages.Values;
    }

    public static ResourceManager Get() {
        if (singleton == null)
            singleton = new ResourceManager();

        return singleton;
    }

    public void Initialize() {
        if (singleton.inited)
            return;

        //
        ReloadAll();

        //
        singleton.inited = true;
    }

    private void ReloadAll() {
        //
        string _folderName = "Json";

        // TODO: 컨테이너 정리 필요
        stages.Clear();

        //
        foreach (string textAssetName in Utility.GetFileNamesAtPath("Patches/" + _folderName)) {
            var ta = Utility.LoadPatches<TextAsset>(_folderName + "/" + textAssetName);
            var parsed = Utility.ParseJSONfromTextAsset(ta);

            foreach (var jStage in parsed) {
                //
                var resStage = new ResourceStage(jStage);
                stages.Add(resStage.id, resStage);
            }

            //
            Resources.UnloadAsset(ta);
            Debug.Log(textAssetName + " : " + parsed.Count);
        }
    }
}
