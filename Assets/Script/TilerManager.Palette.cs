using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class TilerManager {

    //
    [Header("UI Palette")]
    public RuntimePalette runtimePalette;
    public GameObject UIPalette;
    public List<Image> imgStep;
    //public Button btHome;
    //public Button btSave;
    //public Button btChangeBrush;
    //public Button btToggleTool;
    //public Button btClear;

    [Header("Effect")]
    public Image imgTool;
    public Sprite spriteFabric;
    public Animator effectAnimator;
    public Animation animClear;

    [Header("cached")]
    public Vector2 originSize = new Vector2(96, 48);
    public Vector2 widerSize = new Vector2(176, 48);

    //
    private Sequence _seqHideAllPalette;
    private Sequence _usedForPalette;

    public void SetPaletteActive(bool active) {
        runtimePalette.gameObject.SetActive(active);
    }

    public void RefreshPallete(ResourceStage resStage) {
        if (resStage == null)
            return;

        runtimePalette.Initialize(resStage);
        UIPalette.SetActive(true);
    }

    public void OnChangeCanvas() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeCanvas>(parentUI);
        if (popup.GetComponent<Popup_ChangeCanvas>() != null) {
            popup.GetComponent<Popup_ChangeCanvas>().Initialize((width, height) => {
                RuntimePalette.Get().OnChangeCanvasSize(width, height);
            });
        }
    }

    public void OnChangeBrush() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeBrush>(parentUI);
        if (popup.GetComponent<Popup_ChangeBrush>() != null) {
            popup.GetComponent<Popup_ChangeBrush>().Initialize((color, size) => {
                RuntimePalette.Get().OnChangeBrush(color, size);
            });
        }
    }

    public void OnClearPalette() {
        //RuntimePalette.Get().Initialize();
    }

    public void OnSavePalette() {
        // 
        var popup = Utility.InstantiatePrefab<Popup_InputText>(parentUI);
        if (popup.GetComponent<Popup_InputText>() != null) {
            popup.GetComponent<Popup_InputText>().Initialize((name, bytes, width, height) => {

                //
                Dictionary<string, object> map = new Dictionary<string, object>();
                map.Add("name", name);
                map.Add("bytes", Encoding.Unicode.GetString(bytes));
                map.Add("width", width);
                map.Add("height", height);

                //
                Utility.ToJSONfile(name, map);
            }, RuntimePalette.Get().SaveAsBytes(), RuntimePalette.Get().rasterizedTex2D.width, RuntimePalette.Get().rasterizedTex2D.height);
        }
    }

    // =========================================
    public void FadeOutAllPalette(TweenCallback callback) {
        if (_usedForPalette != null)
            _usedForPalette.Kill();

        //
        if (_seqHideAllPalette != null) {
            _usedForPalette = _seqHideAllPalette.OnComplete(callback);
            _usedForPalette.Play();
        }
    }

    public void HideAllPalette(TweenCallback callback) {
        runtimePalette.gameObject.SetActive(false);
        UIPalette.SetActive(false);
    }

    // ===================================================
    // 연출
    // ===================================================
    public void OnClearTray() {
        EmphasizeButton(0);
        effectAnimator.SetTrigger("clear");
    }

    private void EmphasizeButton(int index) {
        if (index < 0 || index >= imgStep.Count)
            return;

        DOTween.Sequence()
            // reset size
            .OnStart(() => {
                imgStep[index].gameObject.SetActive(true);
                foreach (var step in imgStep) {
                    imgStep[index].rectTransform.DOSizeDelta(originSize, 0.5f);
                }
            })
            // resize width
            .Append(imgStep[index].rectTransform.DOSizeDelta(widerSize, 0.5f));
    }
}