using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private bool godMode = false;

    [Header("Refs")] [SerializeField] public UIBehavior ui;
    [SerializeField] Camera cam;
    [SerializeField] public indexer indexer;
    [SerializeField] public map map;
    [SerializeField] public ConnectionManager connecManager;
    [SerializeField] public Button cafeButon;

    [Header("Tags")] [SerializeField] const string treeTag = "Tree";
    [SerializeField] const string grassTag = "Grass";
    [SerializeField] const string traderTag = "Trader";
    [SerializeField] const string beetTag = "Beet";
    [SerializeField] const string groundTag = "Ground";
    [SerializeField] const string stumpTag = "Stump";
    [SerializeField] const string buildingTag = "Building";
    [SerializeField] const string pathTag = "Path";
    [SerializeField] const string bucketBoysTag = "BucketBoys";
    [SerializeField] const string shredderTag = "Shredder";
    [SerializeField] private const string workerTentTag = "WorkerTent";
    const string cafeteriaTag = "Cafeteria";

    [SerializeField] public Color emptyColor;
    [SerializeField] public const string waterResourceTag = "water";
    [SerializeField] public Color waterColor;
    [SerializeField] public const string seedResourceTag = "seed";
    [SerializeField] public Color seedColor;
    [SerializeField] public const string compostResouceTag = "Compost";
    [SerializeField] public Color compostColor;
    [SerializeField] public const string woodResourceTag = "Wood";
    [SerializeField] public Color woodColor;
    [SerializeField] public const string treeSeedResourceTag = "treeSeed";
    [SerializeField] public Color treeSeedColor;


    [Header("Settings")] [SerializeField] public float treeGrowthSmallMaker = 0.01f;
    [SerializeField] private float regularMannequinSpeedBoost = 10f;
    [SerializeField] int regularMannequinSpeedBoostDuration = 5 * 60;

    [FormerlySerializedAs("O2CollectCycle")] [SerializeField]
    private int o2CollectCycle = 5;

    [SerializeField] public int tickTimeSeconds = 10;
    [SerializeField] float oxygenToGoldRatio = 0.2f;
    [SerializeField] float goldToWoodRatio = 2;

    [SerializeField] public GameObject mannequinColliderPrefab;


    private GameObject currentTreePreview;

    private GameObject currentBuildingPreview;
    bool placingTree = false;
    private bool placingBuilding = false;
    int currentTreeID = 0;
    int currentBuildingID = 0;
    private int currentO2 = 100;
    int currentGold = 20;
    int currentWood = 40;
    private bool collectingO2 = true;
    private bool dismantling = false;
    private bool placingPath = false;

    public bool GetPointerDownOnMap()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                return !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            return !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        return false;
    }

    public bool GetPointerUpOnMap()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                bool val = !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                print("Pointer Up on Map: " + val);
                return val;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            bool val = !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            print("Pointer Up on Map: " + val);
            return val;
        }

        return false;
    }

    bool GetPointerClick()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                return !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            return !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        return false;
    }

    void Save()
    {
        map.SaveGame();
    }


    void Start()
    {
        indexer.OnStart();

        StartCoroutine(CollectO2());
        StartCoroutine(GrowTrees());

        UpdateUI();

        map.LoadGame();

        if (godMode)
        {
            currentGold = 999999;
            currentO2 = 999999;
            currentWood = 999999;
        }
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

    public int GetOxygen()
    {
        return currentO2;
    }

    public string GetTreeSeedTag()
    {
        return treeSeedResourceTag;
    }

    public void SetTreeInventory(List<int> treeInventory)
    {
        for (int i = 0; i < indexer.trees.Length; i++)
        {
            indexer.trees[i].amount = treeInventory[i];
        }
    }

    public void SetInventory(int wood, int o2, int gold, List<int> treeInventory)
    {
        currentWood = wood;
        currentO2 = o2;
        currentGold = gold;

        UpdateUI();

        SetTreeInventory(treeInventory);
        ui.RebuildMenus();
    }

    public void AddWood(int amount)
    {
        currentWood += amount;
        ui.UpdateWoodUI(currentWood);
    }

    public string GetBeetTag()
    {
        return beetTag;
    }

    public string GetWoodResourceTag()
    {
        return woodResourceTag;
    }

    public string GetCompostResourceTag()
    {
        return compostResouceTag;
    }

    public string GetWaterResourceTag()
    {
        return waterResourceTag;
    }

    public string GetSeedResourceTag()
    {
        return seedResourceTag;
    }

    public int GetGold()
    {
        return currentGold;
    }

    public void PlaceTree(int treeID)
    {
        ui.camMov.SetMovementLock(true);

        currentTreePreview = Instantiate(indexer.trees[treeID].treeOBJ, ScreenSpaceToWorldPoint(Input.mousePosition),
            GetRandomYRotation());
        currentTreeID = treeID;
        placingTree = true;
    }

    public void AddSeeds(int amount)
    {
        int randomTreeID = Random.Range(0, indexer.trees.Length);
        indexer.trees[randomTreeID].amount += amount;
    }

    public TreeBehavior PlaceTreeByData(TreeData treeData, Beet beet)
    {
        TreeBehavior treeBehavior = Instantiate(indexer.trees[treeData.treeSpeciesId].treeOBJ, treeData.position,
            GetRandomYRotation()).GetComponent<TreeBehavior>();

        treeBehavior.beet = beet;
        treeBehavior.transform.localScale = Vector3.one * treeData.growth;
        treeBehavior.SetOxygenProduction(indexer.trees[treeData.treeSpeciesId].O2Emission);
        treeBehavior.OnPlant(treeData.treeSpeciesId);

        map.AddTree(treeData.treeSpeciesId, treeBehavior.gameObject);

        return treeBehavior;
    }

    public void PlaceBuildingByData(BuildingData buildingData)
    {
        Building bd = Instantiate(indexer.buildings[buildingData.buildingTypeId].buildingObj,
            buildingData.weakTransform.position, buildingData.weakTransform.rotation).GetComponent<Building>();

        bd.SetID(buildingData.buildingTypeId);

        switch (buildingData.buildingTag)
        {
            case bucketBoysTag:
                BucketBoys bb = bd.GetComponent<BucketBoys>();
                bb.SetWater(buildingData.bucketBoysData.water);
                break;
            case workerTentTag:
                civilTent ct = bd.GetComponent<civilTent>();
                ct.SetAssociatedMannequins(buildingData.workerTentData.associatedMannequins);
                break;
            case shredderTag:
                shredder sd = bd.GetComponent<shredder>();
                sd.SetSeeds(buildingData.shredderData.seeds);
                sd.SetCompostResultTag(buildingData.shredderData.compostResultType);
                break;
        }

        map.AddBuilding(buildingData.buildingTypeId, bd.gameObject);
        bd.OnPlaceDown();
    }

    public void PlaceBeetByData(BeetData beetData)
    {
        Beet beet = Instantiate(indexer.buildings[beetData.beetTypeId].buildingObj, beetData.position,
                Quaternion.identity)
            .GetComponent<BeetConnector>().beet;

        beet.SetSeeds(beetData.seeds);
        beet.SetWater(beetData.water);
        beet.beetConnector.OnPlaceDown();

        foreach (TreeData tree in beetData.trees)
        {
            TreeBehavior treeBehavior = PlaceTreeByData(tree, beet);
            beet.AddTree(treeBehavior);
        }

        map.AddBeet(beet);
    }

    public void PlaceBuilding(int id)
    {
        ui.camMov.SetMovementLock(true);

        currentBuildingPreview = Instantiate(indexer.buildings[id].buildingObj,
            ScreenSpaceToWorldPoint(Input.mousePosition), indexer.buildings[id].buildingObj.transform.rotation);
        currentBuildingID = id;
        placingBuilding = true;
    }

    public void PlaceSplineByData(PathData pathData)
    {
        bool firstKnot = true;

        int tempIndex = 0;
        foreach (KnotData knotData in pathData.knots)
        {
            if (tempIndex == 0)
            {
                map.splineInterface.SetFirstPlace(true);
            }

            PlacePathKnotByData(knotData, firstKnot);
            firstKnot = false;
            tempIndex++;
        }
    }

    private float rayAbovePointHeight = 5;
    float autoRayLength = 6;

    public void PlacePathKnotByData(KnotData knotData, bool onNewSpline = false)
    {
        RaycastHit hit;

        /*Ray r = new Ray((knotData.position + knotData.normal.normalized * rayAbovePointHeight),
            -knotData.normal.normalized * autoRayLength);*/ //fancy shit that doesn't work

        Ray r = new Ray(knotData.position + Vector3.up * rayAbovePointHeight, (Vector3.down * autoRayLength));
        Debug.DrawRay(r.origin, r.direction.normalized * autoRayLength, Color.yellow, 10000);

        if (!Physics.Raycast(r, out hit))
        {
            Debug.LogWarning("Auto knot builder error: No hit");
            GameObject noHitErrorObj = new GameObject("No hit error");
            noHitErrorObj.transform.position = r.origin;
            noHitErrorObj.transform.up = r.direction;
            return;
        }

        hit.point = knotData.position;
        bool offset = false;
        bool autoExit = false;


        int connectToSplineIndex = -2;
        var (curspline, isPublicFirstPlace) = GetSplineWithEnds();

        if (hit.collider.gameObject.TryGetComponent<Building>(out Building bd))
        {
            Vector3? point = bd.GetNearestPathConnector(hit.point)?.position;
            if (point == Vector3.zero || point == null) return;
            hit.point = (Vector3)point;
            bd.OnPathConnect(curspline, isPublicFirstPlace);

            autoExit = true;
        }
        else if (hit.collider.gameObject.TryGetComponent<Beet>(out Beet bt))
        {
            Vector3? point = bt.beetConnector.GetNearestPathConnector(hit.point)?.position;
            if (point == Vector3.zero || point == null) return;
            hit.point = (Vector3)point;
            bt.beetConnector.OnPathConnect(curspline, isPublicFirstPlace);
            autoExit = true;
        }
        else if (hit.collider.transform.CompareTag(pathTag))
        {
            int splineIndex = GetSplineIdFromHit(hit);
            var hitSpline = map.splineInterface.splineMesh.LoftSplines[splineIndex];
            connectToSplineIndex = splineIndex;
        }
        else
        {
            print($"Debug hit {hit.transform.name}, Tag: {hit.transform.tag}");
        }

        offset = false; // always false because offset already saved

        int splineID =
            map.splineInterface.AddSplineOrKnot(hit.point, map.GetTerrainNormal(knotData.position), onNewSpline,
                offset: offset,
                connectToSplineIndex: connectToSplineIndex, true);
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

    private Ray r;
    private RaycastHit hit;
    private bool needContninuousRaycast = false;

    void Update()
    {
        needContninuousRaycast = placingTree || placingBuilding;
        if (needContninuousRaycast)
        {
            r = cam.ScreenPointToRay(Input.mousePosition);
        }
        else if (GetPointerDownOnMap())
        {
            r = cam.ScreenPointToRay(Input.mousePosition);
        }

        if (!Physics.Raycast(r, out hit)) return;

        if (placingTree)
        {
            HandleTreePlacement(hit);
        }
        else if (placingBuilding)
        {
            HandleBuildingPlacement(hit);
        }

        if (!GetPointerClick()) return;

        if (dismantling)
        {
            HandleDismantling(hit);
        }
        else if (placingPath)
        {
            HandlePathPlacement(hit);
        }
        else if (!ui.camMov.GetMovementLock())
        {
            HandleOtherClickInteractions(hit);
        }
    }

    void HandleTreePlacement(RaycastHit hit)
    {
        currentTreePreview.transform.position = hit.point;
        if (GetPointerUpOnMap() && !hit.transform.CompareTag(treeTag) && hit.transform.CompareTag(beetTag))
        {
            PlaceTree();

            var beet = hit.collider.gameObject.GetComponent<Beet>();
            var treeBehavior = currentTreePreview.GetComponent<TreeBehavior>();

            treeBehavior.treeIndex = map.GetLastTreeIndex();
            treeBehavior.beet = beet;

            beet.AddTree(treeBehavior);
        }
        else if (GetPointerUpOnMap())
        {
            CancelTreePlacement();
        }
    }

    void HandleBuildingPlacement(RaycastHit hit)
    {
        currentBuildingPreview.transform.position = hit.point;
        if (GetPointerUpOnMap() && hit.transform.CompareTag(groundTag) && !hit.transform.CompareTag(buildingTag))
        {
            PlaceBuilding();
        }
    }

    void HandlePathPlacement(RaycastHit hit)
    {
        if (GetPointerDownOnMap()) //TODO do some tag detection so no paths in weird places ig
        {
            GameObject dbgobj = new GameObject();
            Transform tf = dbgobj.transform;
            tf.position = hit.point;
            tf.eulerAngles = map.GetTerrainNormal(hit.point);
            bool offset = false;
            bool autoExit = false;


            int connectToSplineIndex = -2;
            var (curspline, isPublicFirstPlace) = GetSplineWithEnds();

            if (hit.collider.gameObject.TryGetComponent<Building>(out Building bd))
            {
                Vector3? point = bd.GetNearestPathConnector(hit.point)?.position;
                if (point == Vector3.zero || point == null) return;
                hit.point = (Vector3)point;
                bd.OnPathConnect(curspline, isPublicFirstPlace);

                autoExit = true;
            }
            else if (hit.collider.gameObject.TryGetComponent<Beet>(out Beet bt))
            {
                Vector3? point = bt.beetConnector.GetNearestPathConnector(hit.point)?.position;
                if (point == Vector3.zero || point == null) return;
                hit.point = (Vector3)point;
                bt.beetConnector.OnPathConnect(curspline, isPublicFirstPlace);
                autoExit = true;
            }
            else if (hit.collider.transform.CompareTag(pathTag))
            {
                int splineIndex = GetSplineIdFromHit(hit);
                var hitSpline = map.splineInterface.splineMesh.LoftSplines[splineIndex];
                connectToSplineIndex = splineIndex;
            }
            else
            {
                print($"Debug hit {hit.transform.name}, Tag: {hit.transform.tag}");
            }

            offset = true;

            int splineID =
                map.splineInterface.AddSplineOrKnot(hit.point, map.GetTerrainNormal(hit.point), offset: offset,
                    connectToSplineIndex: connectToSplineIndex);

            Destroy(dbgobj);
            //if(autoExit) ExitPathModeAuto();
        }
    }

    (Spline, bool) GetSplineWithEnds()
    {
        return (map.splineInterface.currentSpline, map.splineInterface.isFirstPlace());
    }

    int GetSplineIdFromHit(RaycastHit hit)
    {
        int triangleIndex = hit.triangleIndex;
        int splineIndex = map.splineInterface.splineMesh.GetSplineIndexFromTriangle(triangleIndex);

        if (splineIndex >= 0)
        {
            Debug.Log($"Hit spline index: {splineIndex.ToString()}");
            return splineIndex;
        }

        return -1;
    }

    void ExitPathModeAuto()
    {
        OnPathPlaceMode(false);
        ui.ShowOnly(ui.screens[0]);
    }

    void HandleDismantling(RaycastHit hit)
    {
        GameObject obj = hit.collider.gameObject;
        string tag = obj.tag;

        if (hit.collider.TryGetComponent<TreeBehavior>(out TreeBehavior tree))
        {
            tree.OnDismantle();
        }
        else if (hit.collider.TryGetComponent<BeetConnector>(out BeetConnector beet))
        {
            beet.OnDismantle();
        }
        else if (hit.collider.TryGetComponent<Building>(out Building building))
        {
            building.OnDismantle();
        }
        else if (tag == pathTag)
        {
            //Debug.LogError("Cannot dismantle path (yet)");
            //TODO ID independent path dismantling, and handle mannequin behavior upon dismantled spline

            Spline splineToDestroy = map.splineInterface.GetSplineFromIndex(
                map.splineInterface.splineMesh.GetSplineIndexFromTriangle(hit.triangleIndex));
            foreach (Mannequin man in map.GetMannequins())
            {
                if (man.spline == splineToDestroy)
                {
                    man.OnSplineDismantled();
                }
            }

            map.splineInterface.RemoveSplineSilent(GetSplineIdFromHit(hit));
        }
    }

    void HandleOtherClickInteractions(RaycastHit hit)
    {
        if (hit.transform == null) return;
        if (hit.transform.CompareTag(grassTag))
        {
            int id = hit.collider.gameObject.GetComponent<GrassBehavior>().OnCollect();
            indexer.trees[id].amount++;
            ui.UpdateAmount(id);
        }
        else if (hit.transform.CompareTag(traderTag))
        {
            ui.OpenTraderMenu();
        }
        else if (hit.transform.CompareTag(stumpTag))
        {
            currentWood += hit.collider.gameObject.GetComponent<stumpBehavior>().OnCollect();
            ui.UpdateWoodUI(currentWood);
        }
        else if (hit.transform.CompareTag(cafeteriaTag))
        {
            hit.collider.gameObject.GetComponent<CafeteriaBuilding>().OnClick();
        }
    }


    void PlaceTree()
    {
        indexer.trees[currentTreeID].amount--;
        ui.UpdateAmount(currentTreeID);
        map.AddTree(currentTreeID, currentTreePreview);
        TreeBehavior tree = currentTreePreview.GetComponent<TreeBehavior>();
        tree.SetOxygenProduction(indexer.trees[currentTreeID].O2Emission);
        tree.OnPlant(currentTreeID);

        RelinquishFromTreePlacement();
    }

    public void OnDismantle(bool dismantle)
    {
        dismantling = dismantle;
    }

    public void OnPathPlaceMode(bool val)
    {
        placingPath = val;
        map.splineInterface.OnPathPlaceMode(val);
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
            yield return new WaitForSeconds(tickTimeSeconds);
            map.GrowTrees();
            Save();
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
        if (gold > currentGold) return;

        currentGold -= gold;
        currentWood += Mathf.RoundToInt(gold * goldToWoodRatio);

        ui.UpdateWoodUI(currentWood);
        ui.UpdateMoneyUI(currentGold);
    }

    private float regularMannequinSpeed = -1;

    public void ApplyMannequinSpeedBoost()
    {
        SpeedBoostMannequins(regularMannequinSpeedBoost, regularMannequinSpeedBoostDuration);
        cafeButon.interactable = false;
    }

    public void SpeedBoostMannequins(float speedBoost, float durationSeconds)
    {
        foreach (Mannequin man in map.GetMannequins())
        {
            if (regularMannequinSpeed == -1f) regularMannequinSpeed = man.speed;
            man.speed += speedBoost;
        }

        StartCoroutine(ResetMannequinSpeedDelayed(regularMannequinSpeed, durationSeconds));
    }

    IEnumerator ResetMannequinSpeedDelayed(float speedBoost, float durationSeconds)
    {
        yield return new WaitForSeconds(durationSeconds);

        cafeButon.interactable = true;
        foreach (Mannequin man in map.GetMannequins())
        {
            man.speed = regularMannequinSpeed;
        }
    }

    private int deleteSaveClicks = 0;

    public void DeleteSaveClick()
    {
        deleteSaveClicks++;

        if (deleteSaveClicks >= 6)
        {
            map.DeleteSave();
            deleteSaveClicks = 0;
            print("DELETED SAVE");
        }
    }
}