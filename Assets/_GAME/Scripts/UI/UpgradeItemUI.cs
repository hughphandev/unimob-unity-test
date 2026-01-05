using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public UpgradeInfo info { get; private set; }
    public Button btn;
    public Image icon;
    public TMP_Text tmpName;
    public TMP_Text tmpDesc;
    public TMP_Text tmpPrice;

    public void Init(UpgradeInfo info)
    {
        this.info = info;
        this.icon.sprite = info.icon;
        this.tmpName.text = info.id;
        this.tmpDesc.text = info.id + "_desc";
        this.tmpPrice.text = Utils.FormatNumber((long)info.price);
    }

    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            GameManager.Instance.ApplyUpgrade(info);
            this.Recycle();
        });
    }
}
