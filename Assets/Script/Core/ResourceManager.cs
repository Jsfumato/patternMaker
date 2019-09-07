using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : MonoBehaviour {

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

        //
        singleton.inited = true;
    }

    private void InitStages() {
        //
        var ta = Utility.LoadResource<TextAsset>("Stages.json");
        var map = Utility.ParseJSON(ta);
        Resources.UnloadAsset(ta);

        //var reader = Utility.Parse (new MemoryStream(ta.bytes));
        //reader.ReadToFollowing("Items");

        //while (reader.Read()) {
        //    if (reader.NodeType == XmlNodeType.Element) {
        //        var n = new SmartXmlNode(reader);
        //        items[long.Parse(n.id)] = new LazyResourceItem(n);

        //        var _type = Utility.ParseEnum(n.GetChildText("Type", string.Empty), ResourceItem.Type.UNKNOWN);
        //        if (_type == ResourceItem.Type.HAIR)
        //            hairIDs.Add(long.Parse(n.id));
        //        else if (_type == ResourceItem.Type.HEAD)
        //            headIDs.Add(long.Parse(n.id));
        //    }
        //}

        ////
        //Resources.UnloadAsset(ta);

        ////
        //ta = Utility.LoadResource<TextAsset>("ShopItems.xml");
        //reader = new XmlTextReader(new MemoryStream(ta.bytes));
        //reader.ReadToFollowing("Items");

        ////while (reader.Read()) {
        ////    if (reader.NodeType == XmlNodeType.Element) {
        ////        var n = new SmartXmlNode(reader);
        ////        cashShopMenu = new ResourceCashShopMenu(n);
        ////        break;
        ////    }
        ////}

        //while (reader.Read()) {
        //    if (reader.NodeType == XmlNodeType.Element) {
        //        var n = new SmartXmlNode(reader);
        //        items[long.Parse(n.id)] = new LazyResourceItem(n);
        //    }
        //}

        ////
        //Resources.UnloadAsset(ta);

        ////
        //ta = Utility.LoadResource<TextAsset>("ClanItems.xml");
        //reader = new XmlTextReader(new MemoryStream(ta.bytes));
        //reader.ReadToFollowing("Items");

        //while (reader.Read()) {
        //    if (reader.NodeType == XmlNodeType.Element) {
        //        var n = new SmartXmlNode(reader);
        //        items[long.Parse(n.id)] = new LazyResourceItem(n);
        //        clanShopItemIDs.Add(long.Parse(n.id));
        //    }
        //}

        //

        //
        // also log the totals. Since these are static, they will be for all runs.
        //		foreach (var total in TinyProfiler.Totals.OrderByDescending(t => t.TotalExecutionTimeInMilliseconds)) {
        //			Debug.Log(string.Format(
        //				"{0}: invoked {1} times, totalling {2}ms, Moving Average={3}ms",
        //				total.Name,
        //				total.TotalInvocationCount,
        //				total.TotalExecutionTimeInMilliseconds,
        //				total.AverageExecutionTimeInMilliseconds
        //			));
        //			// The AverageExecutionTimeInMilliseconds is a moving average over 10 frames, this negates the effects of warmup over time
        //			// To determine the real average just do total.TotalExecutionTimeInMilliseconds / total.TotalInvocationCount
        //		}
    }


    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

        Initialize();
    }
}
