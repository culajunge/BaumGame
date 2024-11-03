using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class UIBehavior : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] public cameraMovement camMov;
    [SerializeField] private GameObject[] screens;
    [SerializeField] private PostProcessVolume ppVolume;
    [SerializeField] private indexer indexer;
    
    [SerializeField] private Transform treeUIParent;
    [SerializeField] GameObject treeUIPrefab;
    
    [SerializeField] Transform buildingUIParent;
    [SerializeField] GameObject buildingUIPrefab;
    
    [SerializeField] Manager manager;
    [SerializeField] Button cancelTreePlacement;
    [SerializeField] Button cancelBuildingPlacement;
    [SerializeField] TMP_Text o2Text;
    [SerializeField] private TMP_Text o2Text2;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text moneyText2;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text woodText2;
    
    
    
    List<UITreeItem> treeUIItems = new List<UITreeItem>();
    List<UIBuildingItem> buildingUIItems = new List<UIBuildingItem>();

    void Start()
    {
        InstantiateTreeUIItems();
        InstantiateBuildingUIItems();
    }

    public void Show(GameObject screen)
    {
        screen.SetActive(true);
    }

    public void ShowOnly(GameObject screen)
    {
        foreach (GameObject s in screens)
        {
            s.SetActive(false);
        }

        screen.SetActive(true);

        bool isHud = screen.name == screens[0].name;

        SetBlur(!isHud);
        camMov.SetMovementLock(!isHud);
    }

    public void SetBlur(bool val)
    {
        ppVolume.enabled = val;
    }

    public void InstantiateTreeUIItems()
    {
        int i = 0;
        foreach (tree t in indexer.trees)
        {
            UITreeItem item = Instantiate(treeUIPrefab, treeUIParent).GetComponent<UITreeItem>();
            item.id = i;
            item.previewImg = t.treePreview;
            item.name = t.treeName;
            item.amount.text = t.amount.ToString();
            item.AddOnClickListener(() => OnTreeItemClicked(item.id));
            item.OnInstance();
            treeUIItems.Add(item);
            i++;
        }
    }

    public void InstantiateBuildingUIItems()
    {
        int i = 0;

        foreach (building b in indexer.buildings)
        {
            UIBuildingItem item = Instantiate(buildingUIPrefab, buildingUIParent).GetComponent<UIBuildingItem>();
            item.id = i;
            item.previewImg = b.buildingPreview;
            item.name = b.buildingName;
            
            item.woodCost.text = b.woodCost.ToString();
            item.moneyCost.text = b.goldCost.ToString();
            item.title.text = b.buildingName;
            
            item.AddOnClickListener(() => OnBuildingItemClicked(item.id));
            
            item.OnInstance();
            
            buildingUIItems.Add(item);
            
            i++;
        }
    }

    public void OnDismantle()
    {
        manager.OnDismantle(true);
    }

    public void FinishDismantle()
    {
        manager.OnDismantle(false);
    }

    public void UpdateAmount(int id)
    {
        treeUIItems[id].amount.text = indexer.trees[id].amount.ToString();
    }

    public void OnTreeItemClicked(int id)
    {

        if (indexer.trees[id].amount <= 0) return;
        
        ShowOnly(screens[0]);
        manager.PlaceTree(id);
        ShowCancelTreePlacementButton(true);
    }

    public void OnBuildingItemClicked(int id)
    {
        if(indexer.buildings[id].woodCost > manager.GetWood() || indexer.buildings[id].goldCost > manager.GetGold()) return;
        
        ShowOnly(screens[0]);
        manager.PlaceBuilding(id);
        ShowCancelBuildingPlacementButton(true);
    }

    public void ShowCancelTreePlacementButton(bool val)
    {
        cancelTreePlacement.gameObject.SetActive(val);
    }

    public void ShowCancelBuildingPlacementButton(bool val)
    {
        cancelBuildingPlacement.gameObject.SetActive(val);
    }

    public void UpdateOxygenUI(int o2)
    {
        o2Text.text = o2.ToString();
        o2Text2.text = o2.ToString();
    }

    public void UpdateMoneyUI(int money)
    {
        moneyText.text = money.ToString();
        moneyText2.text = money.ToString();
    }

    public void UpdateWoodUI(int wood)
    {
        woodText.text = wood.ToString();
        woodText2.text = wood.ToString();
    }

    public void OpenTraderMenu()
    {
        ShowOnly(screens[3]);
        
    }
    
}
