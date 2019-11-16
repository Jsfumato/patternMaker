using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour {
    public SVGImage svg;

    //
    public void Initialize(ResourceStage resStage) {
        svg.sprite = resStage.imgStage;
    }
}
