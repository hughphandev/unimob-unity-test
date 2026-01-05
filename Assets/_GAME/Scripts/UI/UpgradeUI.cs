using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public Button btnClose;
    public UpgradeItemUI upgradeItemPrefab;
    public Transform upgradeHolder;

    void Start()
    {
        foreach (var upgrade in GameManager.Instance.gameData.upgradeData)
        {
            upgradeItemPrefab.Use(upgradeHolder).Init(upgrade);
        }

        btnClose.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void Init()
    {
        this.gameObject.SetActive(true);
    }
}
