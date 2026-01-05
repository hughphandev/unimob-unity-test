using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameData gameData;
    public Building[] currentBuildings;
    public Market market;
    public IngameUI ingameUI;

    public Delivery deliveryPrefab;
    public Customer customerPrefab;

    public float currentCustomerCount = 1;

    public List<Delivery> activeDeliveries = new List<Delivery>();
    public List<Customer> activeCustomers = new List<Customer>();

    public List<UpgradeInfo> currentUpgrades = new List<UpgradeInfo>();

    public long currentGold { get; private set; }

    private void Start()
    {
        foreach (var building in currentBuildings)
        {
            building.Init(gameData.buildingData.First(x => x.id == building.name));
        }

        StartCoroutine(SpawnDelivery(1));
        StartCoroutine(SpawnCustomer(1));

        currentGold = 0;
        ingameUI.UpdateGold(currentGold);
    }

    IEnumerator SpawnDelivery(float inteval)
    {
        while (true)
        {
            if (activeDeliveries.Count < currentBuildings.Count(x => x.currentState == BuildingState.Producing))
            {
                var delivery = deliveryPrefab.Use(market.deliveryStart.position, Quaternion.identity, null);
                delivery.Init(currentBuildings.First(building => building.currentState == BuildingState.Producing && activeDeliveries.All(x => x.production != building)));
                activeDeliveries.Add(delivery);
            }
            yield return new WaitForSeconds(inteval);
        }
    }

    IEnumerator SpawnCustomer(float inteval)
    {
        while (true)
        {
            var index = Array.FindIndex(market.currentCustomers, x => x == null);
            int count = market.currentCustomers.Count(x => x != null);
            if (index >= 0 && count < currentCustomerCount)
            {
                var customer = customerPrefab.Use(market.customerStart.position, Quaternion.identity, null);
                customer.Init(index);
                activeCustomers.Add(customer);
            }
            yield return new WaitForSeconds(inteval);
        }
    }

    public void AddGold(long amount)
    {
        currentGold += amount;
        ingameUI.UpdateGold(currentGold);
    }

    public void ApplyUpgrade(UpgradeInfo info)
    {
        currentUpgrades.Add(info);
        switch (info.type)
        {
            case UpgradeType.Production:
                {
                    foreach (var building in currentBuildings)
                    {
                        if (Utils.OverlapFlag(info.targetTag, building.currentInfo.tag) || building.currentInfo.id == info.additionalTarget)
                        {
                            Utils.ApplyMod(ref building.productInfo.productSellPrice, info.mod, GameManager.Instance.gameData.productData.First(x => x.id == building.productInfo.id).productSellPrice);
                        }
                    }
                }
                break;
            case UpgradeType.Customer:
                Utils.ApplyMod(ref currentCustomerCount, info.mod, market.currentCustomers.Length);
                break;
        }
    }
}
