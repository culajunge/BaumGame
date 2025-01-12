using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIBuildingItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Button button;
    [SerializeField] private Image preview;
    [SerializeField] public TMP_Text title;
    [SerializeField] public TMP_Text woodCost;
    [SerializeField] public TMP_Text moneyCost;

    [HideInInspector] public int id;
    [HideInInspector] public Sprite previewImg;
    UnityAction action;

    private Vector2 imageSize;

    /*
    public void AddOnClickListener(UnityAction action)
    {
        button.onClick.AddListener(action);
    }
*/
    private UnityEvent onPointerDownEvent = new UnityEvent();

    public void AddOnPointerDownListener(UnityAction action)
    {
        onPointerDownEvent.AddListener(action);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDownEvent.Invoke();
    }

    public void OnInstance()
    {
        imageSize = new Vector2(previewImg.rect.width, previewImg.rect.height);
        float x = preview.rectTransform.rect.width;
        float tmp = imageSize.x / x;
        preview.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageSize.y / tmp);
        preview.sprite = previewImg;
    }
}