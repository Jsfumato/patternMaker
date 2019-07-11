using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_ChangeBrush : MonoBehaviour {

    public ColorWheelControl colorControl;
    public delegate void ChangeBrushDelegate(Color color, int size);

    private ChangeBrushDelegate _callback;
    private Color _selected;
    private int _size;

    public void Initialize(ChangeBrushDelegate callback) {
        _callback = callback;
    }

    public void OnOK() {
        var Color = colorControl.Selection;

        if (_callback != null)
            _callback(_selected, _size);

        //
        Destroy(gameObject);
    }

    public void OnCancel() {
        Destroy(gameObject);
    }
}
