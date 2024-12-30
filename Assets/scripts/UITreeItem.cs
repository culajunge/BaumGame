using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITreeItem : MonoBehaviour
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
    
    public void OnInstance()
    {
        preview.sprite = previewImg;
    }
}
