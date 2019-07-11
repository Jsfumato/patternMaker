using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_ChangeCanvas : MonoBehaviour {

    public InputField inputWidth;
    public InputField inputHeight;

    //
    public delegate void ChangeSizeDelegate(int width, int height);
    private ChangeSizeDelegate _callbackOK;

    public void Initialize(ChangeSizeDelegate callbackOK) {
        _callbackOK = callbackOK;

        inputWidth.contentType = InputField.ContentType.DecimalNumber;
        inputHeight.contentType = InputField.ContentType.DecimalNumber;
    }

    public void OnOK() {
        int _width = 128;
        int _height = 128;

        //
        int.TryParse(inputWidth.text, out _width);
        int.TryParse(inputHeight.text, out _height);

        //
        if (_callbackOK != null)
            _callbackOK(_width, _height);

        //
        Destroy(gameObject);
    }

    public void OnCancel() {
        Destroy(gameObject);
    }
}
