using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour {

    private static Vector2 _lastFakeTouchPos;

    public static Touch GetFakeTouch(Vector2 fakePos) {
        Touch fakeTouch = new Touch();
        fakeTouch.fingerId = 10;
        fakeTouch.position = fakePos;
        fakeTouch.deltaTime = Time.deltaTime;
        fakeTouch.deltaPosition = fakeTouch.position - _lastFakeTouchPos;
        fakeTouch.phase = (Input.GetMouseButtonDown(0) ? TouchPhase.Began :
                            (fakeTouch.deltaPosition.sqrMagnitude > 1f ? TouchPhase.Moved : TouchPhase.Stationary));
        fakeTouch.tapCount = 1;

        _lastFakeTouchPos = fakePos;
        return fakeTouch;
    }
    public static GameObject InstantiatePrefab<T>(Transform parent) where T : MonoBehaviour {
        var obj = Resources.Load("Prefab/" + typeof(T).Name, typeof(GameObject)) as GameObject;
        obj = Instantiate(obj);

        //
        obj.SetActive(true);
        obj.transform.SetParent(parent);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.SetAsLastSibling();

        //
        return obj;
    }
}
