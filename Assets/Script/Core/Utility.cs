using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Utility : MonoBehaviour {

    private static Vector2 _lastFakeTouchPos;

    public static Touch GetFakeTouch(Vector2 fakePos) {
        Touch fakeTouch = new Touch();
        fakeTouch.fingerId = 10;
        fakeTouch.position = fakePos;
        fakeTouch.deltaTime = Time.deltaTime;
        fakeTouch.deltaPosition = fakeTouch.position - _lastFakeTouchPos;
        fakeTouch.phase = (Input.GetMouseButtonDown(0) ? TouchPhase.Began :
                            (fakeTouch.deltaPosition.sqrMagnitude > 1f ? TouchPhase.Moved : TouchPhase.Stationary));
        fakeTouch.tapCount = 1;

        _lastFakeTouchPos = fakePos;
        return fakeTouch;
    }

    public delegate void LoadFromLocal<T>(UnityWebRequest req);
    private static IEnumerator _CoLoadFromLocal<T>(UnityWebRequest request, LoadFromLocal<T> success, LoadFromLocal<T> failed) {
        yield return request.SendWebRequest();

        // handle error
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
            failed(request);
            yield break;
        }

        //
        success(request);
    }

    public static byte[] LoadBytes(string path) {

        var www = UnityWebRequest.Get("file://" + path);
        byte[] ret = null;

        //
        CoroutineManager.Get().StartCoroutine(_CoLoadFromLocal<byte[]>(www,
            success: (result) => {
                ret = result.downloadHandler.data;
            },
            failed: (result) => {
                Debug.LogError("Utility::Load<> CANNOT LOAD path : \n" + result.error);
                ret = null;
            }));

        return ret;
    }

    // 무조건 AssetBundle
    public static T LoadPatches<T>(string path) {
        object obj = null;

        Type type = typeof(T);
        Type archivedType = type == typeof(byte[]) || type == typeof(string) ? typeof(TextAsset) : type;
        path = path.Replace('\\', '/');

        ////
        //obj = AssetDatabase.LoadAssetAtPath("Assets/Patches/" + path, archivedType);

        //if (obj == null)
        obj = AssetBundleManager.Get().GetAsset(Path.GetFileName(path), archivedType);

        //
        if (obj == null) {
            Debug.LogWarning("Utility::LoadResource() Failed to load " + path);
            return default(T);
        }

        return ConvertResourceType<T>(path, obj);
    }

    public static T LoadResource<T>(string path) {
        Type type = typeof(T);
        Type archivedType = type == typeof(byte[]) || type == typeof(string) ? typeof(TextAsset) : type;
        path = path.Replace('\\', '/');

        return LoadPatches<T>(path);
    }

    //public static T LoadBytes<T>(string path) {

    //    var www = UnityWebRequest.Get("file://" + path);
    //    // TODO: coroutine으로
    //    GameManager.Get().StartCoroutine(_CoLoadFromLocal<T>(www,
    //        success: (result) => {
    //            var type = typeof(T);

    //            if (type == typeof(byte[]))
    //                return result.downloadHandler.data;
    //            //else if (type == typeof(string))
    //            //    obj = result.downloadHandler.text.Replace("\ufeff", "");
    //            else if (type == typeof(Texture2D))
    //                return ((DownloadHandlerTexture) result.downloadHandler).texture;
    //            else if (type == typeof(AudioClip))
    //                return ((DownloadHandlerAudioClip) result.downloadHandler).audioClip;

    //            return null;
    //        },
    //        failed: (result) => {
    //            Debug.LogError("Utility::Load<> CANNOT LOAD path : \n" + result.error);
    //        }));


    //    var obj = Resources.Load(path, typeof(GameObject)) as GameObject;
    //    if (obj == null) {
    //        return null;
    //    }

    //    obj = Instantiate(obj);

    //    //
    //    obj.SetActive(true);
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    //
    //    return obj;
    //}

    public static GameObject InstantiatePrefab<T>(Transform parent) where T : MonoBehaviour {
        var obj = Resources.Load("Prefab/" + typeof(T).Name, typeof(GameObject)) as GameObject;
        obj = Instantiate(obj);

        //
        obj.SetActive(true);
        obj.transform.SetParent(parent);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.SetAsLastSibling();

        //
        return obj;
    }

    public static List<Dictionary<string, object>> ParseJSONfromTextAsset(TextAsset textAsset) {
        var result = new List<Dictionary<string, object>>();
        if (textAsset == null)
            return result;

        // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
        // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요
        using (StreamReader reader = new StreamReader(new MemoryStream(textAsset.bytes))) {
            Dictionary<string, object> map = null;
            JsonTextReader jsonReader = new JsonTextReader(reader);

            //
            string _lastPropertyName = null;
            while (jsonReader.Read()) {
                switch (jsonReader.TokenType) {
                    // array 시작
                    case JsonToken.StartArray:
                        continue;
                    // array 끝
                    case JsonToken.EndArray:
                        break;

                    // Object 시작
                    case JsonToken.StartObject:
                        map = new Dictionary<string, object>();
                        continue;
                    // Object 끝
                    case JsonToken.EndObject:
                        result.Add(map);
                        continue;

                    // Object 구성
                    case JsonToken.PropertyName:
                        _lastPropertyName = jsonReader.Value.ToString();
                        if (!map.ContainsKey(_lastPropertyName))
                            map.Add(_lastPropertyName, null);
                        break;

                    case JsonToken.Integer:
                    case JsonToken.String:
                    case JsonToken.Bytes:
                        map[_lastPropertyName] = jsonReader.Value;
                        break;
                }
            }
        }

        return result;
    }

    // JSON
    // http://theeye.pe.kr/archives/2736
    //public static Dictionary<string, object> ParseJSONfromTextAsset(TextAsset textAsset) {
    //    if (textAsset == null)
    //        return new Dictionary<string, object>();

    //    // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
    //    // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요
    //    using (StreamReader reader = new StreamReader(new MemoryStream(textAsset.bytes))) {
    //        Dictionary<string, object> map = new Dictionary<string, object>();
    //        JsonTextReader jsonReader = new JsonTextReader(reader);

    //        //
    //        string _lastPropertyName = null;
    //        while (jsonReader.Read()) {
    //            if (jsonReader.Value == null)
    //                continue;

    //            //
    //            if (jsonReader.Value != null) {
    //                if (jsonReader.TokenType == JsonToken.PropertyName) {
    //                    _lastPropertyName = jsonReader.Value.ToString();
    //                    if (!map.ContainsKey(_lastPropertyName))
    //                        map.Add(_lastPropertyName, null);
    //                } else {
    //                    map[_lastPropertyName] = jsonReader.Value;
    //                }
    //            }
    //        }

    //        //
    //        return map;
    //    }
    //}

    public static bool ToJSONfile(string fileName, Dictionary<string, object> mapToSave) {
        if (mapToSave == null)
            return false;

        string path = string.Format("Assets/Resources/Custom/{0}.json", fileName);

        try {
            // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
            // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요
            using (StreamWriter writer = new StreamWriter(path)) {
                Dictionary<string, object> map = new Dictionary<string, object>();
                JsonTextWriter jsonWriter = new JsonTextWriter(writer);

                //
                jsonWriter.WriteStartObject();
                foreach (var entry in mapToSave) {
                    jsonWriter.WritePropertyName(entry.Key);
                    jsonWriter.WriteValue(entry.Value);
                }
                jsonWriter.WriteEndObject();
            }
        } catch {
            // 저장에 실패하면 false 반환
            return false;
        }

#if UNITY_EDITOR
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
#endif

        return true;
    }

    public static byte[] ToByteArray<T>(T obj) {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream()) {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T FromByteArray<T>(byte[] data) {
        if (data == null)
            return default(T);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

        using (MemoryStream ms = new MemoryStream(data)) {
            object obj = bf.Deserialize(ms);
            return (T) obj;
        }
    }

    public static object ObjectFromByteArray(byte[] data) {
        if (data == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data)) {
            return bf.Deserialize(ms);
        }
    }

    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder {
        public override Type BindToType(string assemblyName, string typeName) {
            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            Type typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

            return typeToDeserialize;
        }
    }

    public static bool SaveFile(string directory, string fileName, byte[] bytes) {
        string path = string.Format("{0}/{1}.png", directory, fileName);

        try {
            // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
            // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요

            if (File.Exists(path))
                File.Delete(path);

            var _file = File.Create(path);

            using (BinaryWriter writer = new BinaryWriter(_file)) {
                writer.Write(bytes);
            }
        } catch {
            // 저장에 실패하면 false 반환
            return false;
        }

        //#if UNITY_EDITOR
        //        //Re-import the file to update the reference in the editor
        //        AssetDatabase.ImportAsset(path);
        //#endif

        return true;
    }


    // AssetBundle Manager
    private static System.Collections.IEnumerator PerformLoadResource(WWW www, Type type) {
        yield return www;
    }

    private static UTF8Encoding utf8Encoding = new UTF8Encoding(true);

    // https://3dmpengines.tistory.com/1745
    public static List<string> GetFileNamesAtPath(string path) {

        List<string> result = new List<string>();
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (var filename in fileEntries) {
            //
            if (filename.ToLower().Contains(".meta".ToLower()))
                continue;

            var filename_raw = filename.Replace('\\', '/');
            var str = filename_raw.Split('/');
            filename_raw = str.GetSafe(str.Length - 1);
            Debug.Log(filename_raw);
            result.Add(filename_raw);
        }

        return result;
    }

    //public static T[] GetAtPath<T>(string path) {

    //    ArrayList al = new ArrayList();
    //    string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

    //    //
    //    Debug.Log(fileEntries);

    //    //
    //    foreach (string fileName in fileEntries) {
    //        int assetPathIndex = fileName.IndexOf("Assets");
    //        string localPath = fileName.Substring(assetPathIndex);

    //        UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
    //        if (t != null)
    //            al.Add(t);
    //    }
    //    T[] result = new T[al.Count];
    //    for (int i = 0; i < al.Count; i++)
    //        result[i] = (T)al[i];

    //    return result;
    //}


    //    public static List<string> GetPatchFileNames(string path) {
    //#if UNITY_EDITOR && !UNITY_WEBPLAYER
    //        if (Application.isEditor || !Application.isPlaying)
    //            obj = AssetDatabase.get("Assets/Patches/" + path, archivedType);

    //#elif UNITY_STANDALONE_WIN
    //        if(!path.StartsWith("/")) {
    //#if UNITY_EDITOR
    //            path = Path.Combine(Path.Combine(Application.dataPath, "PatchResources/"), path);
    //#else
    //            path = Path.Combine(Path.Combine(Application.dataPath, "../Client/Assets/PatchResources/"), path);
    //#endif
    //        }

    //        var www = new WWW("file://" + path);
    //        CoroutineManager.Get().StartCoroutine(PerformLoadResource(www, type));
    //        while (!www.isDone) {}

    //        if (type == typeof(byte[]))
    //            obj = www.bytes;
    //        else if (type == typeof(string))
    //            obj = www.text.Replace("\ufeff", "");
    //        else if (type == typeof(Texture2D))
    //            obj = www.texture;
    ////        else if (type == typeof(AudioClip))
    ////			obj = www.audioClip;
    //#endif

    //    }

    //public static 



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
}

public static class ExtensionMethod {

    public static T GetSafe<T>(this T[] arr, int index, T _default = default(T)) {
        if (index >= 0 && index < arr.Length)
            return arr[index];
        return _default;
    }

    public static void SetSafe<T>(this T[] arr, int index, T value = default(T)) {
        if (index >= 0 && index < arr.Length)
            arr[index] = value;
    }

    public static T GetSafe<T>(this IList<T> lst, int index, T _default = default(T)) {
        if (index >= 0 && index < lst.Count)
            return lst[index];
        return _default;
    }

    public static void SetSafe<T>(this IList<T> lst, int index, T value = default(T)) {
        if (index >= 0 && index < lst.Count)
            lst[index] = value;
    }
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Newtonsoft.Json;
//using System.IO;
//using System.Text;
//using System;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Reflection;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class Utility : MonoBehaviour {

//    private static Vector2 _lastFakeTouchPos;

//    public static Touch GetFakeTouch(Vector2 fakePos) {
//        Touch fakeTouch = new Touch();
//        fakeTouch.fingerId = 10;
//        fakeTouch.position = fakePos;
//        fakeTouch.deltaTime = Time.deltaTime;
//        fakeTouch.deltaPosition = fakeTouch.position - _lastFakeTouchPos;
//        fakeTouch.phase = (Input.GetMouseButtonDown(0) ? TouchPhase.Began :
//                            (fakeTouch.deltaPosition.sqrMagnitude > 1f ? TouchPhase.Moved : TouchPhase.Stationary));
//        fakeTouch.tapCount = 1;

//        _lastFakeTouchPos = fakePos;
//        return fakeTouch;
//    }
//    public static GameObject InstantiatePrefab<T>(Transform parent) where T : MonoBehaviour {
//        var obj = Resources.Load("Prefab/" + typeof(T).Name, typeof(GameObject)) as GameObject;
//        obj = Instantiate(obj);

//        //
//        obj.SetActive(true);
//        obj.transform.SetParent(parent);
//        obj.transform.localScale = Vector3.one;
//        obj.transform.localPosition = Vector3.zero;
//        obj.transform.SetAsLastSibling();

//        //
//        return obj;
//    }

//    // JSON
//    // http://theeye.pe.kr/archives/2736
//    public static Dictionary<string, object> ParseJSONfromTextAsset(TextAsset textAsset) {
//        if (textAsset == null)
//            return new Dictionary<string, object>();

//        // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
//        // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요
//        using (StreamReader reader = new StreamReader(new MemoryStream(textAsset.bytes))) {
//            Dictionary<string, object> map = new Dictionary<string, object>();
//            JsonTextReader jsonReader = new JsonTextReader(reader);

//            //
//            string _lastPropertyName = null;
//            while (jsonReader.Read()) {
//                if (jsonReader.Value == null)
//                    continue;

//                //
//                if (jsonReader.Value != null) {
//                    if (jsonReader.TokenType == JsonToken.PropertyName) {
//                        _lastPropertyName = jsonReader.Value.ToString();
//                        if (!map.ContainsKey(_lastPropertyName))
//                            map.Add(_lastPropertyName, null);
//                    } else {
//                        map[_lastPropertyName] = jsonReader.Value;
//                    }
//                }
//            }

//            //
//            return map;
//        }
//    }


//    public static bool ToJSONfile(string fileName, Dictionary<string, object> mapToSave) {
//        if (mapToSave == null)
//            return false;

//        string path = string.Format("Assets/Patches/Stages/{0}.json", fileName);

//        try {
//            // IDisposable을 상속한 TextReader를 상속한 StreamReader는, 해당 스코프를 벗어날 때 자동으로 Dispose를 호출함
//            // TODO: StreamReader 내의 MemoryStream도 자동으로 Dispose 되는지 확인 필요
//            using (StreamWriter writer = new StreamWriter(path)) {
//                Dictionary<string, object> map = new Dictionary<string, object>();
//                JsonTextWriter jsonWriter = new JsonTextWriter(writer);

//                //
//                jsonWriter.WriteStartObject();
//                foreach (var entry in mapToSave) {
//                    jsonWriter.WritePropertyName(entry.Key);
//                    jsonWriter.WriteValue(entry.Value);
//                }
//                jsonWriter.WriteEndObject();
//            }
//        } catch {
//            // 저장에 실패하면 false 반환
//            return false;
//        }

//#if UNITY_EDITOR
//        //Re-import the file to update the reference in the editor
//        AssetDatabase.ImportAsset(path);
//#endif

//        return true;
//    }

//    public static byte[] ToByteArray<T>(T obj) {
//        if (obj == null)
//            return null;
//        BinaryFormatter bf = new BinaryFormatter();
//        using (MemoryStream ms = new MemoryStream()) {
//            bf.Serialize(ms, obj);
//            return ms.ToArray();
//        }
//    }

//    public static T FromByteArray<T>(byte[] data) {
//        if (data == null)
//            return default(T);
//        BinaryFormatter bf = new BinaryFormatter();
//        bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

//        using (MemoryStream ms = new MemoryStream(data)) {
//            object obj = bf.Deserialize(ms);
//            return (T)obj;
//        }
//    }

//    public static object ObjectFromByteArray(byte[] data) {
//        if (data == null)
//            return null;
//        BinaryFormatter bf = new BinaryFormatter();
//        using (MemoryStream ms = new MemoryStream(data)) {
//            return bf.Deserialize(ms);
//        }
//    }

//    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
//    {
//        public override Type BindToType(string assemblyName, string typeName) {
//            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

//            // In this case we are always using the current assembly
//            assemblyName = currentAssembly;

//            // Get the type using the typeName and assemblyName
//            Type typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
//                typeName, assemblyName));

//            return typeToDeserialize;
//        }
//    }

//    // AssetBundle Manager
//    private static System.Collections.IEnumerator PerformLoadResource(WWW www, Type type) {
//        yield return www;
//    }

//    private static UTF8Encoding utf8Encoding = new UTF8Encoding(true);

//    // https://3dmpengines.tistory.com/1745
//    public static List<string> GetFileNamesAtPath(string path) {

//        List<string> result = new List<string>();
//        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
//        foreach (var filename in fileEntries) {
//            //
//            if (filename.ToLower().Contains(".meta".ToLower()))
//                continue;

//            var filename_raw = filename.Replace('\\', '/');
//            var str = filename_raw.Split('/');
//            filename_raw = str.GetSafe(str.Length - 1);
//            Debug.Log(filename_raw);
//            result.Add(filename_raw);
//        }

//        return result;
//    }

//    //public static T[] GetAtPath<T>(string path) {

//    //    ArrayList al = new ArrayList();
//    //    string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

//    //    //
//    //    Debug.Log(fileEntries);

//    //    //
//    //    foreach (string fileName in fileEntries) {
//    //        int assetPathIndex = fileName.IndexOf("Assets");
//    //        string localPath = fileName.Substring(assetPathIndex);

//    //        UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
//    //        if (t != null)
//    //            al.Add(t);
//    //    }
//    //    T[] result = new T[al.Count];
//    //    for (int i = 0; i < al.Count; i++)
//    //        result[i] = (T)al[i];

//    //    return result;
//    //}


//    //    public static List<string> GetPatchFileNames(string path) {
//    //#if UNITY_EDITOR && !UNITY_WEBPLAYER
//    //        if (Application.isEditor || !Application.isPlaying)
//    //            obj = AssetDatabase.get("Assets/Patches/" + path, archivedType);

//    //#elif UNITY_STANDALONE_WIN
//    //        if(!path.StartsWith("/")) {
//    //#if UNITY_EDITOR
//    //            path = Path.Combine(Path.Combine(Application.dataPath, "PatchResources/"), path);
//    //#else
//    //            path = Path.Combine(Path.Combine(Application.dataPath, "../Client/Assets/PatchResources/"), path);
//    //#endif
//    //        }

//    //        var www = new WWW("file://" + path);
//    //        CoroutineManager.Get().StartCoroutine(PerformLoadResource(www, type));
//    //        while (!www.isDone) {}

//    //        if (type == typeof(byte[]))
//    //            obj = www.bytes;
//    //        else if (type == typeof(string))
//    //            obj = www.text.Replace("\ufeff", "");
//    //        else if (type == typeof(Texture2D))
//    //            obj = www.texture;
//    ////        else if (type == typeof(AudioClip))
//    ////			obj = www.audioClip;
//    //#endif

//    //    }

//    //public static 

//    public static T LoadResource<T>(string path) {
//        object obj = null;
//        Type type = typeof(T);

//        Type archivedType = type == typeof(byte[]) || type == typeof(string) ? typeof(TextAsset) : type;

//        path = path.Replace('\\', '/');

//#if UNITY_EDITOR && !UNITY_WEBPLAYER
//        if (Application.isEditor || !Application.isPlaying)
//            obj = AssetDatabase.LoadAssetAtPath("Assets/Patches/Stages/" + path, archivedType);

//#elif UNITY_STANDALONE_WIN
//        if(!path.StartsWith("/")) {
//#if UNITY_EDITOR
//            path = Path.Combine(Path.Combine(Application.dataPath, "PatchResources/"), path);
//#else
//            path = Path.Combine(Path.Combine(Application.dataPath, "../Client/Assets/PatchResources/"), path);
//#endif
//        }

//        var www = new WWW("file://" + path);
//        CoroutineManager.Get().StartCoroutine(PerformLoadResource(www, type));
//        while (!www.isDone) {}

//        if (type == typeof(byte[]))
//            obj = www.bytes;
//        else if (type == typeof(string))
//            obj = www.text.Replace("\ufeff", "");
//        else if (type == typeof(Texture2D))
//            obj = www.texture;
////        else if (type == typeof(AudioClip))
////			obj = www.audioClip;
//#endif

//        //
//        //if (obj == null)
//        //    obj = AssetBundleManager.Get().GetAsset(Path.GetFileName(path), archivedType);

//        //
//        if (obj == null) {
//            Debug.LogWarning("Utility::LoadResource() Failed to load " + path);
//            return default(T);
//        }

//        return ConvertResourceType<T>(path, obj);
//    }

//    private static T ConvertResourceType<T>(string path, object obj) {
//        Type type = typeof(T);

//        //
//        if (obj.GetType() == typeof(TextAsset)) {
//            if (type == typeof(byte[]))
//                return (T) (object) ((TextAsset) obj).bytes;
//            else if (type == typeof(string))
//                return (T) (object) ((TextAsset) obj).text.Replace("\ufeff", "");
//            else if (type == typeof(TextAsset))
//                return (T) (object) ((TextAsset) obj);
//            else
//                Debug.LogWarning("Utility::LoadResource() Not supported TextAsset. path=" + path);
//        }

//        return (T) (object) obj;
//    }
//}

//public static class ExtensionMethod {

//    public static T GetSafe<T>(this T[] arr, int index, T _default = default(T)) {
//        if (index >= 0 && index < arr.Length)
//            return arr[index];
//        return _default;
//    }

//    public static void SetSafe<T>(this T[] arr, int index, T value = default(T)) {
//        if (index >= 0 && index < arr.Length)
//            arr[index] = value;
//    }

//    public static T GetSafe<T>(this IList<T> lst, int index, T _default = default(T)) {
//        if (index >= 0 && index < lst.Count)
//            return lst[index];
//        return _default;
//    }

//    public static void SetSafe<T>(this IList<T> lst, int index, T value = default(T)) {
//        if (index >= 0 && index < lst.Count)
//            lst[index] = value;
//    }
//}