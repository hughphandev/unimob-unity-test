using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.U2D;

public enum ModifierType
{
    Flat,
    Percentage,
    Override,
}

[Serializable]
public struct Modifier
{
    public ModifierType type;
    public float value;
}

[Serializable]
public struct BuildingInfo
{
    public string id;
    public Tag tag;
    public float buildTime;
    public int buildPrice;
    public int baseUpgradePrice;
    public float productionTime;
    public string productId;
    public int productCount;
}

[Serializable]
public struct ProductInfo
{
    public string id;
    public GameObject productGFX;
    public float productSellPrice;
}

public enum UpgradeType
{
    Production,
    Customer,
}

[Flags]
public enum Tag
{
    None = 0,
    Tomato = 1 << 0,
    Everything = ~0,
}

[Serializable]
public struct UpgradeInfo
{
    public string id;
    public Sprite icon;
    public UpgradeType type;

    public Tag targetTag;
    public string additionalTarget;
    public Modifier mod;
    public float price;
}

[CreateAssetMenu(fileName = "GameData", menuName = "GameData", order = 0)]
public class GameData : ScriptableObject
{
    public BuildingInfo[] buildingData;
    public ProductInfo[] productData;
    public UpgradeInfo[] upgradeData;

    public SpriteAtlas ProductAtlas;
    public SpriteAtlas CurrencyAtlas;
}
