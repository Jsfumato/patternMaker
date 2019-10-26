using UnityEngine;
using UnityEngine.UI;

public class Popup_ChangeBrush : MonoBehaviour {

    public ColorWheelControl colorControl;
    public Slider brushSize;
    public delegate void ChangeBrushDelegate(Color color, int size);

    private ChangeBrushDelegate _callback;
    private Color _selected;
    private int _size;

    public void Initialize(ChangeBrushDelegate callback) {
        _callback = callback;
        //
        colorControl.Initialize(400f);
        //
        brushSize.minValue = 1f;
        brushSize.maxValue = 20f;
    }

    public void OnOK() {
        _selected = colorControl.Selection;
        _size = Mathf.RoundToInt(brushSize.value);

        //
        if (_callback != null)
            _callback(_selected, _size);

        //
        Destroy(gameObject);
    }

    public void OnCancel() {
        Destroy(gameObject);
    }
}
