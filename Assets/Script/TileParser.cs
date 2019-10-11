using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

        //
        Color32[] _colors_result_fixed = new Color32[_colors.Length];
        for (int i = 0; i < _colors_result_fixed.Length; i++) {
            _colors_result_fixed[i] = Color.white;
        }

        //
        HashSet<int> _indexChecked = new HashSet<int>();
        HashSet<int> _indexChecked_toRemove = new HashSet<int>();

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
            _indexChecked.Add(i);
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
                _indexChecked.Add(idx);
            }
        }

        // 체크된 픽셀 돌면서, 혼자 떨어진 녀석은 삭제
        int count = 0;
        while (true) {
            bool changed = false;

            foreach (var point in _indexChecked) {
                if (point + origin.width + 1 >= _colors_result.Length)
                    continue;
                if (point - origin.width - 1 < 0)
                    continue;

                //
                var point_n = _colors_result[point + origin.width];
                var point_w = _colors_result[point - 1];
                var point_e = _colors_result[point + 1];
                var point_s = _colors_result[point - origin.width];

                var point_nw = _colors_result[point + origin.width - 1];
                var point_ws = _colors_result[point - origin.width - 1];
                var point_se = _colors_result[point - origin.width + 1];
                var point_ne = _colors_result[point + origin.width + 1];

                //
                int _white_count = 0;
                if (point_n == Color.white)
                    _white_count++;
                if (point_s == Color.white)
                    _white_count++;
                if (point_e == Color.white)
                    _white_count++;
                if (point_w == Color.white)
                    _white_count++;

                //if (point_nw == Color.white)
                //    _white_count++;
                //if (point_ws == Color.white)
                //    _white_count++;
                //if (point_se == Color.white)
                //    _white_count++;
                //if (point_ne == Color.white)
                //    _white_count++;

                if (_white_count >= 3) {
                    changed = true;
                    _colors_result[point] = Color.white;
                    _indexChecked_toRemove.Add(point);
                }
            }
            count++;

            //
            _indexChecked.RemoveWhere(x => _indexChecked_toRemove.Contains(x));
            _indexChecked_toRemove.Clear();

            //
            if (changed == false || count >= 50)
                break;
        }

        //foreach (var p in _indexChecked) {
        //    _colors_result_fixed[p] = Color.black;
        //}

        //// 붙어있는 픽셀의 경우, 중앙에서 가장 먼 픽셀 빼고는 삭제
        //var centerPos = new Vector2(origin.width / 2, origin.height / 2);
        //List<KeyValuePair<float, int>> targets = new List<KeyValuePair<float, int>>();
        //count = 0;
        //_indexChecked_toRemove.Clear();
        //while (true) {
        //    bool changed = false;
        //    foreach (var point in _indexChecked) {
        //        targets.Clear();

        //        if (point + origin.width >= _colors_result.Length)
        //            continue;
        //        if (point - origin.width < 0)
        //            continue;

        //        //
        //        var point_n = _colors_result[point + origin.width];
        //        var point_w = _colors_result[point - 1];
        //        var point_e = _colors_result[point + 1];
        //        var point_s = _colors_result[point - origin.width];

        //        //
        //        var vec = new Vector2(point % origin.width, point / origin.width);
        //        targets.Add(new KeyValuePair<float, int>(Vector2.Distance(vec, centerPos), point));

        //        //
        //        var isNorth = point / origin.width > centerPos.y;
        //        var isWest = point % origin.width < centerPos.x;
        //        var isEast = point % origin.width > centerPos.x;
        //        var isSouth = point / origin.width < centerPos.y;

        //        //
        //        bool canClearCenter_ver = false;
        //        bool canClearCenter_hor = false;
        //        if (point_n == Color.black && isSouth) {
        //            _colors_result[point + origin.width] = Color.white;
        //            if (point_s == Color.black) {
        //                canClearCenter_ver = true;
        //            }
        //        }
        //        if (point_s == Color.black && isNorth) {
        //            _colors_result[point - origin.width] = Color.white;
        //            if (!canClearCenter_ver && point_n == Color.black) {
        //                canClearCenter_ver = true;
        //            }
        //        }
        //        if (point_e == Color.black && isWest) {
        //            _colors_result[point + 1] = Color.white;
        //            if (point_w == Color.black) {
        //                canClearCenter_hor = true;
        //            }
        //        }
        //        if (point_w == Color.black && isEast) {
        //            _colors_result[point - 1] = Color.white;
        //            if (!canClearCenter_hor && point_e == Color.black) {
        //                canClearCenter_hor = true;
        //            }
        //        }

        //        //
        //        if (canClearCenter_ver && canClearCenter_hor)
        //            _colors_result[point] = Color.white;

        //        //
        //        count++;
        //    }

        //    //
        //    if (changed == false || count >= 1)
        //        break;
        //}

        parsedResult.SetPixels32(_colors_result);
        //parsedResult.SetPixels32(_colors_result_fixed);
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

        hSliderValue = GUILayout.HorizontalSlider(hSliderValue, 10f, 150f);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Start Parse")) {
            tp.OnParseTileImage(hSliderValue);
        }

        if (GUILayout.Button("Save Parse")) {
            var _png = tp.parsedResult.EncodeToPNG();
            Utility.SaveFile("Assets/Parser", "test", _png);
        }
    }
}
#endif