using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UITreeItem : MonoBehaviour, IPointerDownHandler
{
    //Hello im hal and im resposible for the ui

    [SerializeField] private Image preview;
    [SerializeField] public TMP_Text amount;
    [HideInInspector] public Sprite previewImg;
    [HideInInspector] public int id;

    [SerializeField] private Button button;

    public void AddOnClickListener(UnityEngine.Events.UnityAction action)
    {
        button.onClick.AddListener(action);
    }

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
        preview.sprite = previewImg;
    }
}