using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // private
    private int oldp;
    private Texture2D myimage;

    private int sw;
    private int sh;

    private float mx;
    private float my;

    private int px;
    private int py;

    private bool _init = false;

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

    void Update() {
#if !UNITY_EDITOR
        if (Input.touchCount <= 0)
            return;

        var touchedPos = Input.GetTouch(0).position;
#else
        if (!Input.GetMouseButton(0))
            return;

        var touchedPos = Utility.GetFakeTouch(Input.mousePosition).position;
#endif

        sw = Screen.width;
        sh = Screen.height;

        mx = touchedPos.x / sw + (rectTrans.localPosition.x - rectTrans.rect.width / 2);
        my = touchedPos.y / sh + (rectTrans.localPosition.y - rectTrans.rect.height / 2);

        px = Mathf.RoundToInt(myimage.width * mx);
        py = Mathf.RoundToInt(myimage.height * my) - 1;

        // <-- only draw when mouse moves for proficiency
        if (px + py != oldp) {
            oldp = px + py;

            px += -Mathf.RoundToInt(brushSize * .5f);
            py += -Mathf.RoundToInt(brushSize * .5f);

            if (_mode == DrawMode.Draw)
                Draw(px, py, drawcolor);
            else if (_mode == DrawMode.Erase)
                Erase(px, py);
        }
    }

    private void Draw(int px, int py, Color color) {
        //
        int _widthIdx = 0;
        while (_widthIdx++ < brushSize) {
            int _heightIdx = 0;
            while (_heightIdx++ < brushSize) {
                //<---dont try to draw off image width
                if ((px + _widthIdx) <= -1 || (px + _widthIdx) >= myimage.width)
                    continue;

                //<---dont try to draw off image height
                if ((py + _heightIdx) <= -1 || (py + _heightIdx) >= myimage.height)
                    continue;

                myimage.SetPixel(px + _widthIdx, py + _heightIdx, color);
            }
        }

        myimage.Apply();
        rawImg.texture = myimage;
    }

    private void Erase(int px, int py) {
        Draw(px, py, Color.white);
    }
}
