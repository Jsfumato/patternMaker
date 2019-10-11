using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ResourceStage : ResourceBase {

    //
    public byte[] bytes;
    public int width;
    public int height;

    public Sprite imgStage;

    //
    public ResourceStage(Dictionary<string, object> jsonRaw) : base(jsonRaw) {
        ////
        //if (jsonRaw.ContainsKey("bytes")) {
        //    bytes = Encoding.Unicode.GetBytes(jsonRaw["bytes"].ToString());
        //}
        ////if (jsonRaw.ContainsKey("bytes")) {
        ////    bytes = Utility.ToByteArray(jsonRaw["bytes"]);
        ////}

        //if (jsonRaw.ContainsKey("width")) {
        //    width = int.Parse(jsonRaw["width"].ToString());
        //}
        //if (jsonRaw.ContainsKey("height")) {
        //    height = int.Parse(jsonRaw["height"].ToString());
        //}

        //
        if (jsonRaw.ContainsKey("sprite")) {
            var _path = jsonRaw["sprite"].ToString();
            imgStage = Utility.LoadResource<Sprite>(_path);
        }
    }
}
