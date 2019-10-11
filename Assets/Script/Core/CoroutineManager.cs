using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour {

    private CoroutineManager() {
    }

    public static CoroutineManager Get() {
        if (_instance == null)
            _instance = new CoroutineManager();
        return _instance;
    }

    private static CoroutineManager _instance;

    public void Awake() {
        DontDestroyOnLoad(this);
    }
}
