using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class CurrentServiceType {
    public const string type = "stage";
    public const string webURL = "http://stg-brm-web-alb-1226725381.ap-northeast-2.elb.amazonaws.com:9980/";
    public const string updateURL_Prefix = "http://updates-kr.supercat.co.kr/kingdomwinds-stage/";
    public const string updateOriginURL_Prefix = "http://updates-kr.supercat.co.kr/kingdomwinds-stage/";
    public const string applicationIdentifier = "com.nexon.baram";

}


public class AssetBundleManager_Custom : AssetBundleManager {
    public override void Awake() {


    }

    private static List<string> GetDownloadableAssetBundleNames() {
//        if (CurrentServiceType.type == ServiceType.DEV)
//            return assetBundleNames;

//#if UNITY_IOS && !UNITY_EDITOR
//        return new List<string>() {
//            "Commons",
//        };
//#endif

        return assetBundleNames;
    }

    public override void FetchStatus(StatusCallback onFinish, bool clearAssetBundles = true) {
        base.FetchStatus((bool success, string message, long fileSize) => {
            if (success) {
                StopAllCoroutines();
                StartCoroutine(DownloadMeta(GetDownloadableAssetBundleNames(), onFinish));
            } else {
                onFinish(false, null, 0);
            }
        }, clearAssetBundles);
    }

    public override void DownloadAll(ProgressCallback onProgress, DownloadCallback onFinish) {
        //
        //if (Config.ignoreAssetBundle) {
        //    if (onFinish != null)
        //        onFinish(true, null);

        //    return;
        //}

        StopAllCoroutines();
        StartCoroutine(Download(GetDownloadableAssetBundleNames(), onProgress, onFinish));
    }

    IEnumerator DownloadMeta(IEnumerable<string> names, StatusCallback onFinish) {
        //
        //if (Config.ignoreAssetBundle) {
        //    if (onFinish != null)
        //        onFinish(true, null, 0);

        //    yield break;
        //}

        string postfix = "";
        string folder = "android/";

//        if (Constants.DEVELOPMENT_MODE) {
//#if UNITY_IPHONE
//			folder = "ios_beta";
//#elif UNITY_ANDROID
//			folder = "android_beta";
//#endif
//        } else {
#if UNITY_IPHONE
			folder = "ios";
#elif UNITY_ANDROID
			folder = "android";
#endif
        //}

        string url = null;
        long fileSize = 0;

        foreach (var name in names) {
            url = CurrentServiceType.updateURL_Prefix + folder + "/" + name + postfix + ".md5?t=" + _version;
            using (var www = new WWW(url)) {
                yield return www;

                if (www.error != null) {
                    Debug.LogWarning(www.error);
                    fileSize = -1;
                    www.Dispose();
                    break;
                }

                string[] contents = www.text.Split('/');
                www.Dispose();

                string configKey = string.Format("AssetHash_{0}", name);
                string crc = contents.GetSafe(2, contents.GetSafe(0));
                if (PlayerPrefs.GetString(configKey, null) == crc)
                    continue;

                if (contents.Length >= 2)
                    fileSize += long.Parse(contents[1]);
            }
        }

        // 
        onFinish(true, null, fileSize);
    }


    IEnumerator Download(IEnumerable<string> names, ProgressCallback onProgress, DownloadCallback onFinish) {

        bool success = true;
        string postfix = "";
        string folder = "android";

//        if (Constants.DEVELOPMENT_MODE) {
//#if UNITY_IPHONE
//			folder = "ios_beta";
//#elif UNITY_ANDROID
//			folder = "android_beta";
//#endif
//        } else {
#if UNITY_IPHONE
			folder = "ios";
#elif UNITY_ANDROID
			folder = "android";
#endif
        //}

        string url = null;
        long fileSize = 0;
        int fileIndex = 0;

        foreach (var name in names) {
            //
            string configKey = string.Format("AssetHash_{0}", name);
            string crc;

            //
            url = CurrentServiceType.updateURL_Prefix + folder + "/" + name + postfix + ".md5?t=" + _version;
            using (var www = new WWW(url)) {
                yield return www;

                if (www.error != null) {
                    Debug.LogWarning(www.error);
                    success = false;
                    break;
                }

                string[] contents = www.text.Split('/');
                crc = contents.GetSafe(2, contents.GetSafe(0));
                Debug.Log(string.Format("Patch CRC {0}.unity3d = {1}", name, crc));

                //
                if (PlayerPrefs.GetString(configKey, null) == crc) {
                    Debug.Log(string.Format("Skipping {0}.unity3d = {1}", name, PlayerPrefs.GetString(configKey, null)));

                    fileIndex++;
                    if (onProgress != null)
                        onProgress(fileIndex / (float) names.Count());
                    continue;
                }

                fileSize = long.Parse(contents[1]);
            }

            //
            var path = Path.Combine(persistentDataPath, name + ".unity3d");
            url = CurrentServiceType.updateURL_Prefix + folder + "/" + name + postfix + ".unity3d?t=" + _version;
            using (var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)) {
                www.downloadHandler = new DownloadHandlerFile(path);

                //
                var op = www.SendWebRequest();
                while (!op.isDone) {
                    yield return null;

                    //
                    if (onProgress != null)
                        onProgress((fileIndex + Mathf.Clamp01(www.downloadedBytes / (float) fileSize)) / (float) names.Count());
                }

                if (www.isNetworkError || www.isHttpError) {
                    Debug.LogWarning(string.Format("Failed to download {0}.unity3d", name));
                    success = false;
                    break;
                }
            }

            //				if(md5.CompareTo(Utility.MD5(bytes)) != 0) {
            //					Debug.LogWarning(string.Format("Failed to download {0}.unity3d", name));
            //					success = false;
            //					break;
            //				}

            try {
#if UNITY_IPHONE
				UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogWarning(string.Format("Failed to save {0}.unity3d", name));
                success = false;
                break;
            }

            fileIndex++;
            if (onProgress != null)
                onProgress(fileIndex / (float) names.Count());
            PlayerPrefs.SetString(configKey, crc);

            //
            Debug.Log(string.Format("Downloaded {0}.unity3d", name));
        }

        PlayerPrefs.Save();
        onFinish(success, null);
    }
}