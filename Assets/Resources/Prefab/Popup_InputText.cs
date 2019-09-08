using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_InputText : MonoBehaviour {

    public InputField inputName;

    //
    public delegate void SavePaletteDelegate(string name, byte[] bytes);
    private SavePaletteDelegate _callbackOK;

    private string _name;
    private byte[] _bytes;

    public void Initialize(SavePaletteDelegate callbackOK, byte[] bytesToSave) {
        _callbackOK = callbackOK;
        _bytes = bytesToSave;

        inputName.contentType = InputField.ContentType.Name;
        inputName.characterLimit = 16;
    }

    public void OnOK() {
        _name = inputName.text;

        //
        if (_callbackOK != null)
            _callbackOK(_name, _bytes);

        //
        Destroy(gameObject);
    }
}
