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

        // stage 정보 세팅
        foreach (ResourceStage resStage in ResourceManager.Get().GetStages()) {
            var cloned = GameObject.Instantiate(cell);
            var stageInfo = cloned.GetComponent<StageCell>();
            if (stageInfo == null)
                stageInfo = cloned.AddComponent<StageCell>();

            // 정보 입력
            stageInfo.txtName.text = resStage.name;
            stageInfo.texture = new Texture2D(resStage.width, resStage.height, TextureFormat.RGBA32, false);
            stageInfo.texture.LoadRawTextureData(resStage.bytes);
            stageInfo.texture.Apply();
            stageInfo.rawImage.texture = stageInfo.texture;

            // 터치하면 퍼즐로 이동
            var btTouched = cloned.GetComponent<Button>();
            if (btTouched == null)
                btTouched = cloned.AddComponent<Button>();
            btTouched.onClick.RemoveAllListeners();
            btTouched.onClick.AddListener(() => {
                TilerManager.Get().editManager.Initialize(resStage);
            });

            // UI에 표기
            cloned.transform.SetParent(table.transform);
            cloned.SetActive(true);
        }
    }
}
