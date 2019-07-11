using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTile : MonoBehaviour {

    //
    public static Transform parentUI;
    public static Transform parentContent;
    public static RuntimePalette _runtimePalette;

    //
    public Transform _parentUI;
    public Transform _parentContent;

    public void Awake() {
        //
        parentUI = _parentUI;
        parentContent = _parentContent;

        //
        _runtimePalette = RuntimePalette.Get();
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
}
