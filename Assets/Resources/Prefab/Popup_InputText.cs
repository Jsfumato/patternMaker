using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_InputText : MonoBehaviour {

    public InputField inputName;

    //
    public delegate void SavePaletteDelegate(string name, byte[] bytes, int width, int height);
    private SavePaletteDelegate _callbackOK;

    private string _name;
    private byte[] _bytes;
    private int _width;
    private int _height;

    public void Initialize(SavePaletteDelegate callbackOK, byte[] bytesToSave, int width, int height) {
        _callbackOK = callbackOK;
        _bytes = bytesToSave;
        _width = width;
        _height = height;

        inputName.contentType = InputField.ContentType.Name;
        inputName.characterLimit = 16;
    }

    public void OnOK() {
        _name = inputName.text;

        //
        if (_callbackOK != null)
            _callbackOK(_name, _bytes, _width, _height);

        //
        Destroy(gameObject);
    }
}
