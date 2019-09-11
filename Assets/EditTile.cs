﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text;

public class EditTile : MonoBehaviour {

    //
    public static Transform parentUI;
    public static Transform parentContent;
    public static RuntimePalette _runtimePalette;

    //
    public Transform _parentUI;
    public Transform _uiStages;
    public Transform _parentContent;


    public void Initialize() {
        //
        parentUI = _parentUI;
        parentContent = _parentContent;

        //
        _runtimePalette = RuntimePalette.Get();
    }

    public void Initialize(ResourceStage resStage) {
        if (resStage == null)
            return;

        Initialize();
        OnLoadPalette(resStage);
    }

    public void OnChangeCanvas() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeCanvas>(parentUI);
        if (popup.GetComponent<Popup_ChangeCanvas>() != null) {
            popup.GetComponent<Popup_ChangeCanvas>().Initialize((width, height) => {
                RuntimePalette.Get().OnChangeCanvasSize(width, height);
            });
        }

        //
        _uiStages.gameObject.SetActive(false);
    }

    public void OnChangeBrush() {
        var popup = Utility.InstantiatePrefab<Popup_ChangeBrush>(parentUI);
        if (popup.GetComponent<Popup_ChangeBrush>() != null) {
            popup.GetComponent<Popup_ChangeBrush>().Initialize((color, size) => {
                RuntimePalette.Get().OnChangeBrush(color, size);
            });
        }

        //
        _uiStages.gameObject.SetActive(false);
    }

    public void OnCreatePalette() {
        RuntimePalette.Get().Initialize(640, 640);

        //
        _uiStages.gameObject.SetActive(false);
    }

    public void OnClearPalette() {
        RuntimePalette.Get().OnClear();
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
            }, RuntimePalette.Get().SaveAsBytes(), RuntimePalette.Get().myimage.width, RuntimePalette.Get().myimage.height);
        }
    }

    public void OnLoadPalette(ResourceStage resStage) {
        //
        _runtimePalette.Initialize(resStage.width, resStage.height);
        _runtimePalette.LoadFromBytes(resStage.bytes);
    }
}
