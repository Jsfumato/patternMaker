using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour {
    //
    public GridLayoutGroup table;
    public GameObject cell;

    public void Initialize() {

        //
        if (cell.activeSelf)
            cell.SetActive(false);

        //
        foreach (ResourceStage stage in ResourceManager.Get().GetStages()) {
            var cloned = GameObject.Instantiate(cell);
            var stageInfo = cloned.GetComponent<StageCell>();
            if (stageInfo == null)
                stageInfo = cloned.AddComponent<StageCell>();

            //
            stageInfo.txtName.text = stage.name;
            stageInfo.texture = new Texture2D(stage.width, stage.height, TextureFormat.RGBA32, false);
            stageInfo.texture.LoadRawTextureData(stage.bytes);
            stageInfo.texture.Apply();

            stageInfo.rawImage.texture = stageInfo.texture;

            //
            cloned.transform.SetParent(table.transform);
            cloned.SetActive(true);
        }
    }
}
