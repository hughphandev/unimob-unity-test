using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngameUI : MonoBehaviour
{
    public BuildingConstructionUI buildingView;
    public BuildingUpgradeUI upgradeView;
    public UpgradeUI upgradeUI;
    public Button btnUpgrade;
    public ConstructionProgressUI constructionProgressPrefab;
    public Transform progressHolder;
    List<ConstructionProgressUI> listProgressUI = new List<ConstructionProgressUI>();
    public TMP_Text tmpGold;

    private void Start()
    {
        btnUpgrade.onClick.AddListener(() =>
        {
            upgradeUI.Init();
        });
    }


    public ConstructionProgressUI GetProgressUI(Building building)
    {
        var progress = constructionProgressPrefab.Use(progressHolder, listProgressUI);
        progress.Init(building);
        return progress;
    }

    public void UpdateGold(long amount)
    {
        tmpGold.text = Utils.FormatNumber(amount);
    }
}
