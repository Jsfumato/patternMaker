using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
[DisallowMultipleComponent]
public class UIDynamicTableView : MonoBehaviour {

    public class RowData {
        public int prefabIdx;
        public float height;

        public RowData(int prefabIdx, float height) {
            this.prefabIdx = prefabIdx;
            this.height = Mathf.Max(0.0f, height);
        }
    }

    //
    public ScrollRect scrollRect;
    public RectTransform contentView;
    public GameObject[] prefabs;

    //
    private bool _initialized = false;
    private Dictionary<int, Queue<GameObject>> _prefabPool = new Dictionary<int, Queue<GameObject>>();
    private List<RowData> _rowData = new List<RowData>();
    private List<GameObject> _cells = new List<GameObject>();

    //
    private int _start = 0;
    private int _end = -1;
    private Vector2 _size;
    private int _totalCount;

    public float GetRowHeight(int idx) {
        if (_rowData.Count >= idx)
            return 0f;

        return _rowData[idx].height;
    }

    public float GetRowOffset(int idx) {
        float result = 0;

        for (int i = 0; i <= _rowData.Count; ++i) {
            if (i >= idx)
                break;

            result += _rowData[i].height;
        }

        return result;
    }

    public float GetTotalRowsHeight() {
        float result = 0;

        for (int i = 0; i <= _rowData.Count; ++i) {
            result += _rowData[i].height;
        }

        return result;
    }

    public void ScrollsToTop() {
        scrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void ScrollsToBottom() {
        scrollRect.content.anchoredPosition = new Vector2(0f, Mathf.Max(0f, scrollRect.content.sizeDelta.y - _size.y));
    }

    public void Initialize(int count, Action refreshCell) {
        if (!_initialized) {
            foreach (var obj in prefabs)
                obj.SetActive(false);

            scrollRect.onValueChanged.AddListener(delegate (Vector2 pos) {
                OnScrollUpdated();
            });

            _initialized = true;
        }

        //
        _totalCount = count;
        _size = GetComponent<RectTransform>().rect.size;

        //
        _start = 0;
        _end = -1;

        //
        OnScrollUpdated();
    }

    private void OnScrollUpdated() {
        if (!_initialized)
            return;

        int newStart = 0;
        int newEnd = 0;

        //
        float offsetY = scrollRect.content.anchoredPosition.y;
        for (int idx = 0; idx < _totalCount; ++idx) {
            if (offsetY <= 0f)
                break;
            //if (idx > 0) {
            //    offsetY -= spacing;
            //} else {
            //    offsetY -= paddingTop;
            //}
            offsetY -= GetRowHeight(idx);
            newStart = Math.Max(idx, 0);
        }

        //
        offsetY = scrollRect.content.anchoredPosition.y;
        for (int idx = 0; idx < _totalCount; ++idx) {
            if (offsetY <= -_size.y)
                break;
            //if (idx > 0) {
            //    offsetY -= spacing;
            //} else {
            //    offsetY -= paddingTop;
            //}
            offsetY -= GetRowHeight(idx);
            newEnd = idx + 1;
        }

        Refresh(newStart, newEnd);
    }

    private void EnqueuePool(int idx, int k) {
        if (idx >= _rowData.Count)
            return;

        var _prefabIdx = _rowData[idx].prefabIdx;
        if (_prefabPool[_prefabIdx] == null)
            _prefabPool[_prefabIdx] = new Queue<GameObject>();

        _prefabPool[_prefabIdx].Enqueue(_cells[k]);
    }

    private GameObject DequeuePool(int _prefabIdx) {
        if (_prefabPool[_prefabIdx] != null && _prefabPool[_prefabIdx].Count > 0)
            return _prefabPool[_prefabIdx].Dequeue();

        return Instantiate(prefabs[_prefabIdx]);
    }

    private void Refresh(int newStart, int newEnd) {
        //
        int k = 0;
        for (int i = _start; i < _end; ++i, ++k) {
            if (!(i >= newStart && i < newEnd)) {
                _cells[k].SetActive(false);
                EnqueuePool(i, k);
            }
        }

        //
        for (int i = _cells.Count - 1; i >= 0; --i) {
            if (!_cells[i].gameObject.activeSelf)
                _cells.RemoveAt(i);
        }

        //
        k = 0;
        for (int _i = newStart; _i < newEnd; ++_i) {

            int i = _i;

            //
            if (i >= _start && i < _end)
                continue;

            //
            GameObject cell = DequeuePool(_rowData[i].prefabIdx);
            cell.SetActive(true);
            cell.transform.SetParent(scrollRect.content.transform, false);

            //
            var rt = (cell.transform as RectTransform);
            rt.anchoredPosition = new Vector2(0f, -GetRowOffset(i));

            //
            if (i >= _end)
                _cells.Add(cell);
            else if (i < _start) {
                _cells.Insert(k++, cell);
            }
        }

        //
        _start = newStart;
        _end = newEnd;

        scrollRect.content.sizeDelta = new Vector2(0, GetTotalRowsHeight());
    }

}
