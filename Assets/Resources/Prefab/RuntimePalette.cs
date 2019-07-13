﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RuntimePalette : MonoBehaviour
{
    //
    public RectTransform rectTrans;
    public RawImage rawImg;
    public int width;
    public int height;
    public Texture2D original;

    public int brushSize;
    public int eraseSize;

    public Color drawcolor;

    public bool fillmode = false;

    // private
    private Vector2 oldp = Vector2.zero;
    private Texture2D myimage;

    private bool _init = false;
    private List<List<Vector2>> _logs = new List<List<Vector2>>();
    private List<Vector2> _currentLog = new List<Vector2>();

    private Color32[] cur_colors;
    private bool _drawing = false;

    //
    private static RuntimePalette _palette;

    public static RuntimePalette Get() {
        if (_palette == null) {
            _palette = Utility.InstantiatePrefab<RuntimePalette>(EditTile.parentContent)
                .GetComponent<RuntimePalette>();
            _palette.Initialize();
        }
        return _palette;
    }

    public enum DrawMode
    {
        Draw = 0,
        Erase
    }

    private DrawMode _mode;

    public void Initialize() {
        if (_init)
            return;

        if (original == null) {
            original = new Texture2D(width, height);
        }

        // change these next three variables to whatever you want!!!
        drawcolor = Color.red;
        brushSize = 6;
        eraseSize = 20;

        //copy our original into our new paintable image 
        myimage = new Texture2D(original.width, original.height);
        rectTrans.sizeDelta = new Vector2(original.width, original.height);
        rectTrans.pivot = Vector2.one * 0.5f;
        rectTrans.localPosition = Vector3.zero;

        //
        int _widthIdx = original.width;
        while (_widthIdx-- > 0) {
            int _heightIdx = original.width;
            while (_heightIdx-- > 0) {
                myimage.SetPixel(
                    _widthIdx, _heightIdx,
                    original.GetPixel(_widthIdx, _heightIdx)
                    );
            }
        }

        //
        myimage.Apply();
        myimage.filterMode = FilterMode.Point;//<remove this if you want it more fuzzy

        //
        rawImg.texture = myimage;

        //
        _init = true;
    }

    public void OnChangeCanvasSize(int width, int height) {
        myimage = new Texture2D(width, height);
        rectTrans.sizeDelta = new Vector2(width, height);
        rectTrans.pivot = Vector2.one * 0.5f;
        rectTrans.localPosition = Vector3.zero;

        //
        int _widthIdx = original.width;
        while (_widthIdx-- > 0) {
            int _heightIdx = original.width;
            while (_heightIdx-- > 0) {
                myimage.SetPixel(
                    _widthIdx, _heightIdx,
                    original.GetPixel(_widthIdx, _heightIdx)
                    );
            }
        }

        //
        myimage.Apply();
        myimage.filterMode = FilterMode.Point;//<remove this if you want it more fuzzy

        //
        rawImg.texture = myimage;
    }

    public void OnChangeBrush(Color brushColor, int brushSize) {
        this.drawcolor = brushColor;
        this.brushSize = brushSize;
    }

    private bool _touched = false;
    void Update() {
        //
        if (!_touched)
            return;

#if !UNITY_EDITOR
        if (Input.touchCount <= 0)
            return;

        var touchedPos = Input.GetTouch(0).position;
#else
        if (!Input.GetMouseButton(0))
            return;

        var touchedPos = Utility.GetFakeTouch(Input.mousePosition).position;
#endif

        Vector2 dir = new Vector2(rectTrans.position.x, rectTrans.position.y) - touchedPos;

        //
        if ((Mathf.Abs(dir.x) >= rectTrans.rect.width / 2)
            || (Mathf.Abs(dir.y) >= rectTrans.rect.height / 2)) {
            _touched = false;
            return;
        }

        //
        int _px = Mathf.RoundToInt(rectTrans.rect.width * ((rectTrans.rect.width / 2 - dir.x) / rectTrans.rect.width));
        int _py = Mathf.RoundToInt(rectTrans.rect.height * ((rectTrans.rect.height / 2 - dir.y) / rectTrans.rect.height));

        int _oldX = Mathf.RoundToInt(oldp.x);
        int _oldY = Mathf.RoundToInt(oldp.y);

        //
        if (fillmode) {
            FloodFill(myimage, new Vector2(_px, _py), drawcolor);
            return;
        }


        // <-- only draw when mouse moves for proficiency
        if (_px == _oldX && _py == _oldY)
            return;

        if (oldp == Vector2.zero)
            oldp = new Vector2(_px, _py);

        _px += -Mathf.RoundToInt(brushSize * .5f);
        _py += -Mathf.RoundToInt(brushSize * .5f);

        if (_mode == DrawMode.Draw)
            Draw(_px, _py, brushSize, drawcolor);
        else if (_mode == DrawMode.Erase)
            Erase(_px, _py);

        //
        _currentLog.Add(new Vector2(_px, _py));
    }

    public void OnPointerDown() {
        _touched = true;
        _currentLog.Clear();

        //
        cur_colors = myimage.GetPixels32();
    }

    public void OnPointerUP() {
        _touched = false;

        //
        _logs.Add(_currentLog);
        if (_logs.Count > 20)
            _logs.RemoveAt(0);

        //
        oldp = Vector2.zero;

        //
        myimage.SetPixels32(cur_colors);
        myimage.Apply();

        //rawImg.texture = myimage;
    }

    private void Draw(int px, int py, int width, Color color) {
        StartCoroutine(CoColourBetween(oldp, new Vector2(px, py), width, color));

        //
        oldp = new Vector2(px, py);

        ////
        //int _widthIdx = 0;
        //while (_widthIdx++ < brushSize) {
        //    int _heightIdx = 0;
        //    while (_heightIdx++ < brushSize) {
        //        //<---dont try to draw off image width
        //        if ((px + _widthIdx) <= -1 || (px + _widthIdx) >= myimage.width)
        //            continue;

        //        //<---dont try to draw off image height
        //        if ((py + _heightIdx) <= -1 || (py + _heightIdx) >= myimage.height)
        //            continue;

        //        //
        //        myimage.SetPixel(px + _widthIdx, py + _heightIdx, color);

        //    }
        //}
    }

    private void Erase(int px, int py) {
        Draw(px, py, 10, Color.white);
    }

    //===========================================
    public IEnumerator CoColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color) {
        // Get the distance from start to finish
        float distance = Vector2.Distance(start_point, end_point);

        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
        float lerp_steps = 1 / distance;

        //
        for (float lerp = 0; lerp <= 1; lerp += lerp_steps) {
            var cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }

        //
        myimage.SetPixels32(cur_colors);
        myimage.Apply();

        yield break;
    }

    //public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color) {
    //    // Get the distance from start to finish
    //    float distance = Vector2.Distance(start_point, end_point);
    //    Vector2 direction = (start_point - end_point).normalized;

    //    Vector2 cur_position = start_point;

    //    // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
    //    float lerp_steps = 1 / distance;

    //    for (float lerp = 0; lerp <= 1; lerp += lerp_steps) {
    //        cur_position = Vector2.Lerp(start_point, end_point, lerp);
    //        MarkPixelsToColour(cur_position, width, color);
    //    }
    //}

    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen) {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        //int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++) {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= (int)myimage.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++) {
                MarkPixelToChange(x, y, color_of_pen);
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color) {
        // Need to transform x and y coordinates to flat coordinates of array
        int array_pos = y * myimage.width + x;

        // Check if this is a valid position
        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

        cur_colors[array_pos] = color;
    }

    //===================================================
    void FloodFill(Texture2D image, Vector2 point, Color color) {
        Color32[] _colors = image.GetPixels32();

        // Create WestEast

        //
        var m_List = new Queue<Vector2>();
        var m_WestEast = new List<int>();

        //
        //get pixel 1 to left (w) of Q[n]
        int _array_pos_base = Mathf.RoundToInt(point.y) * image.width + Mathf.RoundToInt(point.x);
        // Check if this is a valid position
        if (_array_pos_base > _colors.Length || _array_pos_base < 0)
            return;

        //
        m_List.Enqueue(point);
        var _colToCompare = _colors[_array_pos_base];

        // east
        var _array_pos = _array_pos_base;
        int _newX = Mathf.RoundToInt(point.x);
        int _newY = Mathf.RoundToInt(point.y);
        while (++_newX <= image.width) {
            _array_pos = _newY * image.width + _newX;
            Color wCol = _colors[_array_pos];
            if (wCol == _colToCompare) {
                m_List.Enqueue(new Vector2(_newX, _newY));
            } else {
                break;
            }
        }
        // west
        _array_pos = _array_pos_base;
        _newX = Mathf.RoundToInt(point.x);
        _newY = Mathf.RoundToInt(point.y);
        while (--_newX >= 0) {
            _array_pos = _newY * image.width + _newX;
            Color wCol = _colors[_array_pos];
            if (wCol == _colToCompare) {
                m_List.Enqueue(new Vector2(_newX, _newY));
            } else {
                break;
            }
        }

        //
        foreach (var _p in m_List) {
            _array_pos = Mathf.RoundToInt(_p.y) * image.width + Mathf.RoundToInt(_p.x);
            _colors[_array_pos] = color;

            //
            //11.If the color of the node to the north of n is target-color, add that node to Q.
            _newX = Mathf.RoundToInt(point.x);
            _newY = Mathf.RoundToInt(point.y + 1);
            _array_pos = _newY * image.width + _newX;
            if (_array_pos < _colors.Length && _colors[_array_pos] != color) {
                m_List.Enqueue(new Vector2());
            }

            //
            //12.If the color of the node to the south of n is target - color, add that node to Q.
            _newX = Mathf.RoundToInt(point.x);
            _newY = Mathf.RoundToInt(point.y - 1);
            _array_pos = _newY * image.width + _newX;
            if (_array_pos >= 0 && _colors[_array_pos] != color) {
                m_List.Enqueue(new Vector2());
            }
        }
    }
}