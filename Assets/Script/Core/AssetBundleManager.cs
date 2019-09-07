using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AssetBundleManager : MonoBehaviour {

    //
    public delegate void StatusCallback(bool success, string message, long fileSize);
    public delegate void DownloadCallback(bool success, string message);
    public delegate void ProgressCallback(float v);

    #region PersistentDataPath
    private static string[] _persistentDataPaths;

    private static string GetPersistentDataPath(params string[] components) {
        try {
            string path = Path.DirectorySeparatorChar + string.Join("" + Path.DirectorySeparatorChar, components);
            if (!Directory.GetParent(path).Exists)
                return null;
            if (!Directory.Exists(path)) {
                Debug.Log("creating directory: " + path);
                Directory.CreateDirectory(path);
            }
            if (!IsDirectoryWritable(path)) {
                Debug.LogWarning("persistent data path not writable: " + path);
                return null;
            }
            return path;
        } catch {
            return null;
        }
    }

    public static string persistentDataPathInternal {
#if UNITY_ANDROID
        get
        {
            if (Application.isEditor || !Application.isPlaying) return Application.persistentDataPath;
            string path = null;
            if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "emulated", "0", "Android", "data", Application.identifier, "files");
            if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("data", "data", Application.identifier, "files");
            return path;
        }
#else
        get {
            return Application.persistentDataPath;
        }
#endif
    }

    public static string persistentDataPathExternal {
#if UNITY_ANDROID
        get
        {
            if (Application.isEditor || !Application.isPlaying) return null;
            string path = null;
            if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "sdcard0", "Android", "data", Application.identifier, "files");
            if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "sdcard1", "Android", "data", Application.identifier, "files");
            if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("mnt", "sdcard", "Android", "data", Application.identifier, "files");
            return path;
        }
#else
        get {
            return null;
        }
#endif
    }

    public static string[] persistentDataPaths {
        get {
            if (_persistentDataPaths == null) {
                List<string> paths = new List<string>();
                if (!string.IsNullOrEmpty(persistentDataPathInternal))
                    paths.Add(persistentDataPathInternal);
                if (!string.IsNullOrEmpty(persistentDataPathExternal))
                    paths.Add(persistentDataPathExternal);
                if (!string.IsNullOrEmpty(Application.persistentDataPath) && !paths.Contains(Application.persistentDataPath))
                    paths.Add(Application.persistentDataPath);
                _persistentDataPaths = paths.ToArray();
            }
            return _persistentDataPaths;
        }
    }

    // returns best persistent data path
    public static string persistentDataPath {
        get {
            return persistentDataPaths.Length > 0 ? persistentDataPaths[0] : null;
        }
    }
    #endregion

    public static bool IsDirectoryWritable(string path) {
        try {
            if (!Directory.Exists(path))
                return false;
            string file = Path.Combine(path, Path.GetRandomFileName());
            using (FileStream fs = File.Create(file, 1)) {
            }
            File.Delete(file);
            return true;
        } catch {
            return false;
        }
    }

    //
    public static List<string> assetBundleNames = new List<string>() {
        "Stages"
    };

    //
    private static AssetBundleManager _singleton;
    public static AssetBundleManager Get() {
        if (!_singleton) {
            if (!Application.isPlaying)
                return null;

            var obj = new GameObject("[AssetBundleManager]");
            DontDestroyOnLoad(obj);

            _singleton = obj.AddComponent<AssetBundleManager_Custom>();
        }
        return _singleton;
    }

    //
    [NonSerialized]
    public AssetBundleInfo commonsAssetBundle;

    public class AssetBundleInfo {
        public Stream stream;
        public AssetBundle assetBundle;

        internal AssetBundleInfo(AssetBundle assetBundle, Stream stream) {
            this.stream = stream;
            this.assetBundle = assetBundle;
        }

        public void Destroy() {
            try {
                if (assetBundle != null)
                    assetBundle.Unload(true);
                assetBundle = null;
            } catch {

            }

            try {
                if (stream != null)
                    stream.Dispose();
                stream = null;
            } catch {

            }
        }
    }

    //
    protected readonly List<AssetBundleInfo> assetBundles = new List<AssetBundleInfo>();
    protected uint _version;
    public bool loadedAll = false;

    public void UnloadCommonAssetBundle() {
        if (commonsAssetBundle != null) {
            commonsAssetBundle.Destroy();
            assetBundles.Remove(commonsAssetBundle);
            commonsAssetBundle = null;
        }
    }

    public virtual void Awake() {

        //
        var cache = Caching.currentCacheForWriting;
        cache.expirationDelay = 12960000;
        cache.maximumAvailableStorageSpace = 4 * 1024 * 1024 * 1024L;
    }

    private void Clear() {
        foreach (string name in assetBundleNames) {
            string configKey = string.Format("AssetHash_{0}", name);
            PlayerPrefs.SetString(configKey, "");
        }
        PlayerPrefs.Save();
        Caching.ClearCache();
    }

    public virtual void FetchStatus(StatusCallback onFinish, bool clearAssetBundles = true) {
        if (onFinish != null) {
            //
            if (clearAssetBundles) {
                foreach (var ab in assetBundles)
                    ab.Destroy();
                assetBundles.Clear();
            }

            //
            onFinish(true, null, 0);
        }
    }
    
    public void LoadAll(Action<bool, string> _onFinish, ProgressCallback onProgress) {

        Action<bool> onFinish = delegate (bool success) {

            if (success == false) {
                if (_onFinish != null)
                    _onFinish(false, null);
                return;
            }

            //
            try {
                ResourceManager.Get().Initialize();

                //
                UnloadCommonAssetBundle();
            } catch (Exception e) {
                Debug.LogWarning(e);

                if (_onFinish != null) {
                    _onFinish(false, e.ToString());
                }
                return;
            }

            if (_onFinish != null)
                _onFinish(success, null);
            return;
        };

        ////
        //if (Config.ignoreAssetBundle) {
        //    if (onFinish != null)
        //        onFinish(true);

        //    return;
        //}

        //
        StartCoroutine(
            Load(assetBundleNames, delegate (bool success) {
                if (onFinish != null)
                    onFinish(success);
            }, onProgress));
    }

    IEnumerator Load(IEnumerable<string> names, Action<bool> onFinish, ProgressCallback onProgress) {

        yield return null;
        yield return Resources.UnloadUnusedAssets();

        //
        foreach (var ab in assetBundles)
            ab.Destroy();
        assetBundles.Clear();

        if (onProgress != null)
            onProgress(0f);

        //
        bool success = true;
        int i = 0;
        var nameCount = names.Count();
        foreach (var name in names) {
            //
            Stream stream = null;
            AssetBundleCreateRequest req = null;

            //// 일단 StreamingAssets 폴더에서 가져오기를 시도한다.
            //try {
            //    string path = Path.Combine(Application.streamingAssetsPath, "AssetBundles/" + name + ".unity3d");

            //    stream = new XorStream(new FileStream(path, FileMode.Open, FileAccess.Read), KEY);
            //    req = AssetBundle.LoadFromStreamAsync(stream);
            //} catch {
            //}

            // 로딩 시도!
            if (req != null)
                yield return req;

            //
            if (req != null && req.assetBundle) {
                // PASS
            } else {

                try {
                    if (stream != null)
                        stream.Close();
                } catch {

                }

                //// persistentDataPath 폴더에서 가져오기를 시도한다.
                //try {
                //    string path = this is AssetBundleManager_NXPatcher ? NXPatcher.GetFilePath(name + ".unity3d")
                //        : Path.Combine(persistentDataPath, name + ".unity3d");

                //    stream = new XorStream(new FileStream(path, FileMode.Open, FileAccess.Read), KEY);
                //    req = AssetBundle.LoadFromStreamAsync(stream);
                //} catch {
                //}

                // 로딩 시도!
                if (req != null)
                    yield return req;
            }

            try {
                if (req != null && req.assetBundle != null) {
                    req.assetBundle.name = name;

                    //
                    var abInfo = new AssetBundleInfo(req.assetBundle, stream);

                    //
                    if (name == "Commons")
                        commonsAssetBundle = abInfo;

                    //
                    assetBundles.Add(abInfo);
                    Debug.Log(string.Format("Loaded {0}.unity3d", name));

                    if (onProgress != null)
                        onProgress(++i / (float) nameCount);
                } else {
                    try {
                        if (stream != null)
                            stream.Close();
                    } catch {

                    }

                    //
                    success = false;

                    string configKey = string.Format("AssetHash_{0}", name);
                    PlayerPrefs.SetString(configKey, "");
                    Debug.LogWarning(string.Format("Failed to load {0}.unity3d", name));
                    break;
                }
            } catch (Exception e) {
                Debug.LogException(e);
                success = false;

                //
                string configKey = string.Format("AssetHash_{0}", name);
                PlayerPrefs.SetString(configKey, "");
                Debug.LogWarning(string.Format("Failed to load {0}.unity3d", name));
                break;
            }
        }

        //
        if (success)
            loadedAll = true;
        onFinish(success);

        yield break;
    }


    public virtual void DownloadAll(ProgressCallback onProgress, DownloadCallback onFinish) {

    }

    public UnityEngine.Object GetAsset(string name, Type type) {
        foreach (var ab in assetBundles) {
            var obj = ab.assetBundle.LoadAsset(name, type);
            if (obj)
                return obj;
        }

        return null;
    }

    public void GetAssetAsync(string name, Type type, Action<UnityEngine.Object> callback) {
        StartCoroutine(GetAssetCoroutine(name, type, callback));
    }

    public IEnumerator GetAssetCoroutine(string name, Type type, Action<UnityEngine.Object> callback) {
        var args = name.Split('/');
        string filterAssetBundle = "Commons";
        string fileName = name;

        if (args.Length >= 2) {
            filterAssetBundle = args[0];
            fileName = args[args.Length - 1];
        }

        foreach (var ab in assetBundles) {
            // 애셋번들을 필터링 해야할 경우,
            if (filterAssetBundle != null && string.Compare(ab.assetBundle.name, filterAssetBundle, true) != 0)
                continue;

            //
            var req = ab.assetBundle.LoadAssetAsync(fileName, type);
            yield return req;

            if (req.asset) {
                callback(req.asset);
                yield break;
            }
        }

        callback(null);
    }

}