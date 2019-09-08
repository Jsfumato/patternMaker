using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ResourceStage : ResourceBase {

    //
    public new string name;
    public byte[] bytes;

    public ResourceStage(Dictionary<string, object> jsonRaw) : base() {
        //
        if (jsonRaw.ContainsKey("name")) {
            name = jsonRaw["name"].ToString();
        }

        //
        if (jsonRaw.ContainsKey("bytes")) {
            bytes = Encoding.UTF8.GetBytes(jsonRaw["bytes"].ToString());
        }
    }
}
