using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIBuildingItem : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] private Image preview;
    [SerializeField] public TMP_Text title;
    [SerializeField] public TMP_Text woodCost;
    [SerializeField] public TMP_Text moneyCost;

    [HideInInspector] public int id;
    [HideInInspector] public Sprite previewImg;

    public void AddOnClickListener(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void OnInstance()
    {
        preview.sprite = previewImg;
    }
}