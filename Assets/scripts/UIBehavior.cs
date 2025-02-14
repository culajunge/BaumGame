using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIBehavior : MonoBehaviour
{
    [Header("Refs")] [SerializeField] public cameraMovement camMov;

    //[SerializeField] public PanAndZoom camMov;
    [SerializeField] public GameObject[] screens;
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

    [SerializeField] Animator toolsMenuAnimator;
    [SerializeField] private float toggleAnimationClipLength = 15f / 60f;


    List<UITreeItem> treeUIItems = new List<UITreeItem>();
    List<UIBuildingItem> buildingUIItems = new List<UIBuildingItem>();
    private static readonly int Open = Animator.StringToHash("open");

    void Start()
    {
        //InstantiateTreeUIItems();
        //InstantiateBuildingUIItems();

        animators.Add(toolsMenuAnimator);
    }

    public void CreateUiItemsUponNoLoad()
    {
        InstantiateBuildingUIItems();
        InstantiateTreeUIItems();
    }

    public void ClearMenus()
    {
        foreach (UITreeItem item in treeUIItems)
        {
            Destroy(item.gameObject);
        }

        treeUIItems.Clear();

        foreach (UIBuildingItem item in buildingUIItems)
        {
            Destroy(item.gameObject);
        }

        buildingUIItems.Clear();
    }

    public void RebuildMenus()
    {
        ClearMenus();
        InstantiateTreeUIItems();
        InstantiateBuildingUIItems();
    }

    private List<Animator> animators = new List<Animator>();

    public void ToggleToolsMenu()
    {
        bool isToolsMenuOpen = toolsMenuAnimator.GetBool(Open);

        if (!isToolsMenuOpen)
        {
            // Opening tools menu - close all other menus first
            CloseAllExcept(toolsMenuAnimator.gameObject);
            ToggleMenu(toolsMenuAnimator, true, true);
        }
        else
        {
            // Closing tools menu - close all sub-menus too
            CloseAllMenus();
        }
    }

    public void ToggleToolsMenu2()
    {
        ToggleMenu(toolsMenuAnimator);
    }

    public void ToggleSubMenu(Animator submenuAnimator)
    {
        // Only allow toggling submenu if tools menu is open
        if (toolsMenuAnimator.GetBool(Open))
        {
            ToggleMenu(submenuAnimator);
        }
        else
        {
            // First open tools menu, then open submenu
            ToggleMenu(toolsMenuAnimator, true, true);
            StartCoroutine(OpenSubmenuNextFrame(submenuAnimator));
        }
    }

    // Replace your existing VToggleMenu with this
    public void VToggleMenu(Animator animator)
    {
        // If it's the main tools menu, use ToggleToolsMenu behavior
        if (animator == toolsMenuAnimator)
        {
            ToggleToolsMenu();
        }
        // Otherwise treat it as a submenu
        else
        {
            ToggleSubMenu(animator);
        }
    }

    private IEnumerator OpenSubmenuNextFrame(Animator submenuAnimator)
    {
        yield return null;
        ToggleMenu(submenuAnimator, true, true);
    }

    // Your existing methods remain the same...
    public void CloseAllMenus()
    {
        foreach (Animator animator in animators)
        {
            ToggleMenu(animator, true, false);
        }
    }

    public void CloseAllExcept(GameObject obj)
    {
        foreach (Animator animator in animators)
        {
            if (animator.gameObject == obj) continue;
            ToggleMenu(animator, true, false);
        }
    }

    bool ToggleMenu(Animator animator, bool shouldOverrideVal = false, bool overrideVal = false)
    {
        bool newState = shouldOverrideVal ? overrideVal : !animator.GetBool(Open);

        if (newState)
        {
            animator.gameObject.SetActive(true);
            StartCoroutine(SetAnimatorBoolNextFrame(animator, "open", true));
        }
        else
        {
            animator.SetBool(Open, false);
            ToggleMenuActivity(animator, toggleAnimationClipLength, false);
        }

        if (!animators.Contains(animator))
        {
            animators.Add(animator);
        }

        return newState;
    }

    private IEnumerator SetAnimatorBoolNextFrame(Animator animator, string paramName, bool value)
    {
        // Wait for the next frame
        yield return null;
        animator.SetBool(paramName, value);
    }

    public void ToggleBuildMenu(Animator animator)
    {
        ToggleMenu(animator);
        ToggleMenu(toolsMenuAnimator);
    }

    public bool ToggleMenuActivity(Animator animator, float clipLength, bool val)
    {
        bool state = val;
        if (state)
        {
            animator.gameObject.SetActive(true);
        }
        else
        {
            StartCoroutine(SetInactiveAfterTimeS(animator.gameObject, clipLength));
        }

        return state;
    }

    IEnumerator SetInactiveAfterTimeS(GameObject obj, float s)
    {
        yield return new WaitForSeconds(s);
        obj.SetActive(false);
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
            item.AddOnPointerDownListener(() => OnTreeItemPointerDown(item.id));
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

            //item.AddOnClickListener(() => OnBuildingItemClicked(item.id));
            item.AddOnPointerDownListener(() => OnBuildingItemPointerDown(item.id));

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

    public void OnTreeItemPointerDown(int id)
    {
        if (indexer.trees[id].amount <= 0) return;

        ShowOnly(screens[0]);
        manager.PlaceTree(id);
        ShowCancelTreePlacementButton(true);
    }

    public void OnBuildingItemClicked(int id)
    {
        if (indexer.buildings[id].woodCost > manager.GetWood() ||
            indexer.buildings[id].goldCost > manager.GetGold()) return;

        ShowOnly(screens[0]);
        manager.PlaceBuilding(id);
        ShowCancelBuildingPlacementButton(true);
    }

    public void OnBuildingItemPointerDown(int id)
    {
        if (indexer.buildings[id].woodCost > manager.GetWood() ||
            indexer.buildings[id].goldCost > manager.GetGold()) return;

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