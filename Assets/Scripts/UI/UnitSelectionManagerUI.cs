using System;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
        UnitSelectionManager.Instance.OnSelectionAreaStart += OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += OnSelectionAreaEnd;
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    private void OnSelectionAreaEnd(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void OnSelectionAreaStart(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();
        float canvasScale = canvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x,selectionAreaRect.y)/canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width,selectionAreaRect.height)/canvasScale;
    }
}
