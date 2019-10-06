﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileParser : MonoBehaviour
{
    public Texture2D origin;
    public Texture2D parsedResult;

    public void OnParseTileImage(float weight) {
        if (origin == null)
            return;

        //
        parsedResult = ParseOutline(origin, weight);
    }

    private Texture2D ParseOutline(Texture2D origin, float weight) {

        // 컬러 정보를 가져와서
        Color32[] _colors = origin.GetPixels32();
        var parsedResult = new Texture2D(origin.width, origin.height, TextureFormat.RGBA32, false);

        //
        Color32[] _colors_result = new Color32[_colors.Length];
        for (int i = 0; i < _colors_result.Length; i++) {
            _colors_result[i] = Color.white;
        }

        // 가로로 서칭하면서 색상 정보 비교
        for (int i = 0; i < _colors.Length; ++i) {
            if (i > 0 && i % origin.width == 0)
                continue;

            if ((i + 1) >= _colors.Length)
                break;

            //
            var _current = _colors[i];
            var _next = _colors[i + 1];

            //
            var _diff_r = Mathf.Abs(_current.r - _next.r);
            var _diff_g = Mathf.Abs(_current.g - _next.g);
            var _diff_b = Mathf.Abs(_current.b - _next.b);

            //
            if ((_diff_r + _diff_g + _diff_b) < weight)
                continue;

            _colors_result[i] = Color.black;
        }

        // 세로로 서칭하면서 색상 정보 비교
        // 각 column에 대해
        for (int i = 0; i < origin.width; ++i) {
            // 세로로 서칭하면서
            for (int j = 0; j < origin.height; ++j) {
                int idx = i + j * origin.width;
                int idx_next = i + (j + 1) * origin.width;

                if (idx >= _colors.Length || idx_next >= _colors.Length)
                    break;

                //
                var _current = _colors[idx];
                var _next = _colors[idx_next];

                //
                var _diff_r = Mathf.Abs(_current.r - _next.r);
                var _diff_g = Mathf.Abs(_current.g - _next.g);
                var _diff_b = Mathf.Abs(_current.b - _next.b);

                //
                if ((_diff_r + _diff_g + _diff_b) < weight)
                    continue;

                _colors_result[idx] = Color.black;
            }
        }

        parsedResult.SetPixels32(_colors_result);
        parsedResult.Apply();

        return parsedResult;
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(TileParser))]
public class TileParserEditor : Editor {
    public float hSliderValue;

    public override void OnInspectorGUI() {
        TileParser tp = (TileParser) target;

        //
        DrawDefaultInspector();
        EditorGUILayout.Space();

        hSliderValue = GUILayout.HorizontalSlider(hSliderValue, 0f, 100f);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Start Parse")) {
            tp.OnParseTileImage(hSliderValue);
        }

        if (GUILayout.Button("Save Parse")) {
            var _png = tp.parsedResult.EncodeToPNG();
            Utility.SaveFile("test", _png);
        }
    }
}
#endif