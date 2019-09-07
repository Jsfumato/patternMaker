using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : MonoBehaviour {

    const string PATCH_FOLDER_NAME = "Patches";
    const string DIR_PATCHES = "Assets/" + PATCH_FOLDER_NAME + "/";


    public void Initialize() {

    }
    private static System.Collections.IEnumerator PerformLoadResource(WWW www, Type type) {
        yield return www;
    }

    private static UTF8Encoding utf8Encoding = new UTF8Encoding(true);
    public static T LoadResource<T>(string path) {
        object obj = null;
        Type type = typeof(T);

        Type archivedType = type == typeof(byte[]) || type == typeof(string) ? typeof(TextAsset) : type;

        path = path.Replace('\\', '/');

#if UNITY_EDITOR && !UNITY_WEBPLAYER
        if (Application.isEditor || !Application.isPlaying)
            obj = AssetDatabase.LoadAssetAtPath("Assets/PatchResources/" + path, archivedType);

#elif UNITY_STANDALONE_WIN
        if(!path.StartsWith("/")) {
#if UNITY_EDITOR
            path = Path.Combine(Path.Combine(Application.dataPath, "PatchResources/"), path);
#else
            path = Path.Combine(Path.Combine(Application.dataPath, "../Client/Assets/PatchResources/"), path);
#endif
        }

        var www = new WWW("file://" + path);
        CoroutineManager.Get().StartCoroutine(PerformLoadResource(www, type));
        while (!www.isDone) {}

        if (type == typeof(byte[]))
            obj = www.bytes;
        else if (type == typeof(string))
            obj = www.text.Replace("\ufeff", "");
        else if (type == typeof(Texture2D))
            obj = www.texture;
//        else if (type == typeof(AudioClip))
//			obj = www.audioClip;
#endif

        //
        //if (obj == null)
        //    obj = AssetBundleManager.Get().GetAsset(Path.GetFileName(path), archivedType);

        //
        if (obj == null) {
            Debug.LogWarning("Utility::LoadResource() Failed to load " + path);
            return default(T);
        }

        return ConvertResourceType<T>(path, obj);
    }

    private static T ConvertResourceType<T>(string path, object obj) {
        Type type = typeof(T);

        //
        if (obj.GetType() == typeof(TextAsset)) {
            if (type == typeof(byte[]))
                return (T) (object) ((TextAsset) obj).bytes;
            else if (type == typeof(string))
                return (T) (object) ((TextAsset) obj).text.Replace("\ufeff", "");
            else if (type == typeof(TextAsset))
                return (T) (object) ((TextAsset) obj);
            else
                Debug.LogWarning("Utility::LoadResource() Not supported TextAsset. path=" + path);
        }

        return (T) (object) obj;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        Initialize();
    }
}
