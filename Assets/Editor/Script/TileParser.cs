using System.Collections;
using UnityEngine;
using UnityEditor;

public class TileParser : MonoBehaviour {
    public Texture2D origin;
    public Texture2D parsedResult;

    public void OnParseTileImage() {
        if (origin == null)
            return;

        //
        parsedResult = new Texture2D(origin.width, origin.height, TextureFormat.RGBA32, false);

        //

    }
}
