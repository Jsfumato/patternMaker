using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text;
using DG.Tweening;

public class EditManager : MonoBehaviour {

    //
    public static RuntimePalette _runtimePalette;

    [Header("Button")]
    public Button btHome;
    public Button btSave;
    public Button btChangeBrush;
    public Button btToggleTool;
    public Button btClear;

    //
    private Sequence _seqHideAll;
    private Sequence _used;

    public void Initialize() {

        //
        _runtimePalette = RuntimePalette.Get();
        _runtimePalette.transform.SetParent(TilerManager.Get().parentContent);
        _runtimePalette.gameObject.SetActive(false);

        // 버튼 세팅
        btHome.onClick.RemoveAllListeners();
        btHome.onClick.AddListener(() => {
            TilerManager.Get().FadeOutAll();
            TilerManager.Get().lobby.Initialize(true);
        });

        //
        btChangeBrush.onClick.RemoveAllListeners();
        btChangeBrush.onClick.AddListener(OnChangeBrush);

        //
        btClear.onClick.RemoveAllListeners();
        btClear.onClick.AddListener(OnClearPalette);

        //
        btSave.onClick.RemoveAllListeners();
        btSave.onClick.AddListener(OnSavePalette);

        //
        btToggleTool.onClick.RemoveAllListeners();
        btToggleTool.onClick.AddListener(_runtimePalette.OnToggleBrushMode);
    }

    public void SetPaletteActive(bool active) {
        _runtimePalette.gameObject.SetActive(active);
    }

    public void Initialize(int width, int height) {
        Initialize();
        _runtimePalette.Initialize(width, height);
    }

    public void Initialize(ResourceStage resStage) {
        if (resStage == null)
            return;

        Initialize();
        OnLoadPalette(resStage);
    }

    public void OnChangeCanvas() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeCanvas>(TilerManager.Get().parentUI);
        if (popup.GetComponent<Popup_ChangeCanvas>() != null) {
            popup.GetComponent<Popup_ChangeCanvas>().Initialize((width, height) => {
                RuntimePalette.Get().OnChangeCanvasSize(width, height);
            });
        }
    }

    public void OnChangeBrush() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeBrush>(TilerManager.Get().parentUI);
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
        var popup = Utility.InstantiatePrefab<Popup_InputText>(TilerManager.Get().parentUI);
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

    public void OnLoadPalette(ResourceStage resStage) {
        _runtimePalette.Initialize(resStage);
    }

    // =========================================
    public void FadeOutAll(TweenCallback callback) {
        if (_used != null)
            _used.Kill();

        //
        if (_seqHideAll != null) {
            _used = _seqHideAll.OnComplete(callback);
            _used.Play();
        }
    }

}
