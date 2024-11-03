using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class Manager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] UIBehavior ui;
    [SerializeField] Camera cam;
    [SerializeField] public indexer indexer;
    [SerializeField] map map;

    [Header("Tags")]
    [SerializeField] const string treeTag = "Tree";
    [SerializeField] const string grassTag = "Grass";
    [SerializeField] const string traderTag = "Trader";
    [SerializeField] const string beetTag = "Beet";
    [SerializeField] const string groundTag = "Ground";
    [SerializeField] const string stumpTag = "Stump";
    [SerializeField] const string buildingTag = "Building";
    
    [Header("Settings")]
    [SerializeField] public float treeGrowthSmallMaker = 0.01f;
    [FormerlySerializedAs("O2CollectCycle")] [SerializeField] private int o2CollectCycle = 5;
    [SerializeField] private int treeGrowCycleSeconds = 10;
    [SerializeField] float oxygenToGoldRatio = 0.2f;
    [SerializeField] float goldToWoodRatio = 2;

    

    private GameObject currentTreePreview;
    
    private  GameObject currentBuildingPreview;
    bool placingTree = false;
    private bool placingBuilding = false;
    int currentTreeID = 0;
    int currentBuildingID = 0;
    private int currentO2 = 100;
    int currentGold = 20;
    int currentWood = 40;
    private bool collectingO2 = true;
    private bool dismantling = false;
    

    void Start()
    {
        StartCoroutine(CollectO2());
        StartCoroutine(GrowTrees());
        
        UpdateUI();
    }

    void UpdateUI()
    {
        ui.UpdateOxygenUI(currentO2);
        ui.UpdateMoneyUI(currentGold);
        ui.UpdateWoodUI(currentWood);
    }

    public int GetWood()
    {
        return currentWood;
    }

    public string GetBeetTag()
    {
        return beetTag;
    }

    public int GetGold()
    {
        return currentGold;
    }
    public void PlaceTree(int treeID)
    {
        ui.camMov.SetMovementLock(true);
        
        currentTreePreview = Instantiate(indexer.trees[treeID].treeOBJ, ScreenSpaceToWorldPoint(Input.mousePosition), GetRandomYRotation());
        currentTreeID = treeID;
        placingTree = true;
    }
    
    public void PlaceBuilding(int id)
    {
        ui.camMov.SetMovementLock(true);
        
        currentBuildingPreview = Instantiate(indexer.buildings[id].buildingObj, ScreenSpaceToWorldPoint(Input.mousePosition), indexer.buildings[id].buildingObj.transform.rotation);
        currentBuildingID = id;
        placingBuilding = true;
    }
    
    Quaternion GetRandomYRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    Vector3 ScreenSpaceToWorldPoint(Vector3 screenPosition)
    {
        return cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, screenPosition.z));
    }

    public void CancelTreePlacement()
    {
        placingTree = false;
        Destroy(currentTreePreview);
        RelinquishFromTreePlacement();
    }

    public void CancelBuildingPlacement()
    {
        placingBuilding = false;
        Destroy(currentBuildingPreview);
        RelinquishFromBuildingPlacement();
    }
    //this is a test change for git
    void RelinquishFromTreePlacement()
    {
        placingTree = false;
        ui.camMov.SetMovementLock(false);
        ui.ShowCancelTreePlacementButton(false);
    }

    void RelinquishFromBuildingPlacement()
    {
        placingBuilding = false;
        ui.camMov.SetMovementLock(false);
        ui.ShowCancelBuildingPlacementButton(false);
    }

    void Update()
    {
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            if (placingTree)
            {
                currentTreePreview.transform.position = hit.point;
                
                if (Input.GetMouseButtonDown(0) && !hit.transform.CompareTag(treeTag) && hit.transform.CompareTag(beetTag))
                {
                    PlaceTree();

                    Beet beet = hit.collider.gameObject.GetComponent<Beet>();
                    TreeBehavior treeBehavior = currentTreePreview.GetComponent<TreeBehavior>();
                    
                    treeBehavior.treeIndex = map.GetLastTreeIndex();
                    treeBehavior.beet = beet;
                    
                    beet.AddTree(treeBehavior);
                    
                    //hit.collider.gameObject.GetComponent<BeetConnector>().AddTree(currentTreePreview.GetComponent<TreeBehavior>());
                }
            }else if (placingBuilding)
            {
                
                currentBuildingPreview.transform.position = hit.point;

                if (Input.GetMouseButtonDown(0) && hit.transform.CompareTag(groundTag))
                {
                    PlaceBuilding();
                }
                
            }else if (dismantling && Input.GetMouseButtonDown(0))
            {
                print("Dismantle");
                GameObject o = hit.collider.gameObject;
                string tag = o.tag;

                switch (tag)
                {
                    case treeTag: 
                        o.GetComponent<TreeBehavior>().OnDismantle();
                        break;
                    case buildingTag:
                        o.GetComponent<Building>().OnDismantle();
                        break;
                    case beetTag:
                        o.GetComponent<Beet>().OnDismantle();
                        break;

                }
            }else if (Input.GetMouseButtonDown(0) && !ui.camMov.GetMovementLock())
            {
                if (hit.transform.CompareTag(grassTag))
                {
                    int id = hit.collider.gameObject.GetComponent<GrassBehavior>().OnCollect();
                    indexer.trees[id].amount++;
                    ui.UpdateAmount(id);
                }else if (hit.transform.CompareTag(traderTag))
                {
                    ui.OpenTraderMenu();
                }else if (hit.transform.CompareTag(stumpTag))
                {
                    currentWood += hit.collider.gameObject.GetComponent<stumpBehavior>().OnCollect();
                    ui.UpdateWoodUI(currentWood);
                }
            }
        }
        
    }

    void PlaceTree()
    {
        indexer.trees[currentTreeID].amount--;
        ui.UpdateAmount(currentTreeID);
        map.AddTree(currentTreeID, currentTreePreview);
                    
        currentTreePreview.GetComponent<TreeBehavior>().OnPlant(currentTreeID);
                    
        RelinquishFromTreePlacement();
    }

    public void OnDismantle(bool dismantle)
    {
        dismantling = dismantle;
    }

    public void PaybackBuilding(int id)
    {
        currentWood += indexer.buildings[id].woodCost;
        currentGold += indexer.buildings[id].goldCost;

        ui.UpdateWoodUI(currentWood);
        ui.UpdateMoneyUI(currentGold);
    }

    public void PaybackTree(int id)
    {
        print("Nah we dont do tis");
    }
    

    void PlaceBuilding()
    {
        currentWood -= indexer.buildings[currentBuildingID].woodCost;
        currentGold -= indexer.buildings[currentBuildingID].goldCost;
        
        ui.UpdateWoodUI(currentWood);
        ui.UpdateMoneyUI(currentGold);
        
        map.AddBuilding(currentBuildingID, currentBuildingPreview);

        BeetConnector bc;
        currentBuildingPreview.TryGetComponent<BeetConnector>(out bc);
        if (bc != null)
        {
            map.AddBeet(bc.GetBeet());
        }

        Building b = currentBuildingPreview.GetComponent<Building>();
        b.buildingIndex = map.GetLastBuildingIndex();
        b.SetID(currentBuildingID);
        b.OnPlaceDown();
        
        RelinquishFromBuildingPlacement();
    }

    IEnumerator CollectO2()
    {
        while (collectingO2)
        {
            yield return new WaitForSeconds(o2CollectCycle);
            currentO2 += map.GetO2Emission();
            ui.UpdateOxygenUI(currentO2);
        }
    }

    IEnumerator GrowTrees()
    {
        while (true)
        {
            yield return new WaitForSeconds(treeGrowCycleSeconds);
            map.GrowTrees();
        }
    }

    public void O2ToGold(int o2)
    {
        if (o2 > currentO2) return;
        currentO2 -= o2;
        currentGold += Mathf.RoundToInt(o2 * oxygenToGoldRatio);
        
        ui.UpdateOxygenUI(currentO2);
        ui.UpdateMoneyUI(currentGold);
    }

    public void GoldToWood(int gold)
    {
        if(gold > currentGold) return;
        
        currentGold -= gold;
        currentWood += Mathf.RoundToInt(gold * goldToWoodRatio);
        
        ui.UpdateWoodUI(currentWood);
        ui.UpdateMoneyUI(currentGold);
    }

}
