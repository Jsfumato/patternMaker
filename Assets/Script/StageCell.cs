using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour {
    public Text txtName;
    public SVGImage svg;

    //
    public void Initialize(ResourceStage resStage) {
        txtName.text = resStage.name;
        svg.sprite = resStage.imgStage;
    }
}
