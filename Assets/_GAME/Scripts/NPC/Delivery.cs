using System.Linq;
using DG.Tweening;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.AI;

public enum DeliveryState
{
    GotoProduction,
    WaitForProduction,
    GotoMarket,
    WaitForCustomer,
    Return,
}

public interface IDeliveryState
{
    public void OnEnter(Delivery owner);
    public void OnUpdate(Delivery owner);
    public void OnExit(Delivery owner);
}

public class Delivery : MonoBehaviour
{
    public DeliveryState currentState;
    public Building production;
    public Animator anim;
    public NavMeshAgent agent;
    public Transform carryTrans;
    public int marketSlot;

    IDeliveryState[] states = new IDeliveryState[]
    {
        new DeliveryGoToProduction(),
        new DeliveryWaitForProduction(),
        new DeliveryGoToMarket(),
        new DeliveryWaitForCustomer(),
        new DeliveryReturn(),
    };

    public void Init(Building production)
    {
        this.production = production;
        this.currentState = DeliveryState.GotoProduction;
        states[(int)this.currentState].OnEnter(this);
    }


    public void SwitchState(DeliveryState newState)
    {
        if (currentState != newState)
        {
            states[(int)currentState].OnExit(this);
            this.currentState = newState;
            states[(int)currentState].OnEnter(this);
        }
    }

    private void Update()
    {
        states[(int)currentState].OnUpdate(this);
    }
}

public class DeliveryGoToProduction : IDeliveryState
{
    public void OnEnter(Delivery owner)
    {
        owner.anim.Play("Move");
        owner.agent.SetDestination(owner.production.deliveryPoint.position);
    }

    public void OnExit(Delivery owner)
    {
    }

    public void OnUpdate(Delivery owner)
    {
        if (Vector3.Distance(owner.transform.position, owner.production.deliveryPoint.position) < 0.1f)
        {
            owner.SwitchState(DeliveryState.WaitForProduction);
        }
    }
}

public class DeliveryWaitForProduction : IDeliveryState
{
    public void OnEnter(Delivery owner)
    {
        owner.anim.Play("Idle");
        owner.agent.isStopped = true;
        owner.transform.LookAt(new Vector3(owner.production.transform.position.x, owner.transform.position.y, owner.production.transform.position.z));
    }

    public void OnExit(Delivery owner)
    {
        owner.agent.isStopped = false;
    }

    public void OnUpdate(Delivery owner)
    {


        if (owner.carryTrans.childCount == GameConstant.maxProductCarries)
        {
            owner.marketSlot = GameManager.Instance.market.GetDeliveryEmptySlot();
            if (owner.marketSlot >= 0)
            {
                owner.SwitchState(DeliveryState.GotoMarket);
            }
        }
        else
        {
            foreach (var trans in owner.production.productPlaceHolders)
            {
                if (trans.childCount != 0)
                {
                    var product = trans.GetChild(0);
                    product.SetParent(owner.carryTrans);
                    product.DOLocalMove(new Vector3(0, GameConstant.carryOffset * product.GetSiblingIndex(), 0), 0.5f);
                }
            }
        }
    }
}

public class DeliveryGoToMarket : IDeliveryState
{
    public void OnEnter(Delivery owner)
    {
        owner.anim.Play("CarryMove");
        owner.agent.SetDestination(GameManager.Instance.market.deliverySlots[owner.marketSlot].position);
        GameManager.Instance.market.currentDeliveries[owner.marketSlot] = owner;
    }

    public void OnExit(Delivery owner)
    {
    }

    public void OnUpdate(Delivery owner)
    {
        if (Vector3.Distance(owner.transform.position, GameManager.Instance.market.deliverySlots[owner.marketSlot].position) < 0.1f)
        {
            owner.SwitchState(DeliveryState.WaitForCustomer);
        }
    }
}

public class DeliveryWaitForCustomer : IDeliveryState
{
    public void OnEnter(Delivery owner)
    {
        owner.anim.Play("CarryIdle");
        owner.agent.isStopped = true;
        owner.transform.LookAt(new Vector3(GameManager.Instance.market.customerSlots[owner.marketSlot].position.x, owner.transform.position.y, GameManager.Instance.market.customerSlots[owner.marketSlot].position.z));
    }

    public void OnExit(Delivery owner)
    {
        owner.agent.isStopped = false;
    }

    public void OnUpdate(Delivery owner)
    {
        var customer = GameManager.Instance.market.currentCustomers[owner.marketSlot];
        if (customer != null && customer.currentState == CustomerState.WaitForDelivery)
        {
            for (int i = 0; i < owner.carryTrans.childCount; ++i)
            {
                var product = owner.carryTrans.GetChild(i);
                product.DOMove(customer.carryTrans.position + new Vector3(0, i * GameConstant.carryOffset), 0.5f).OnComplete(() =>
                {
                    product.SetParent(customer.carryTrans);
                    var goldEarn = owner.production.productInfo.productSellPrice * (1 + owner.production.currentLevel * GameConstant.productionRatePerLevel);
                    Debug.Log($"goldEarn: {goldEarn}, id: {owner.production.currentInfo.productId}");
                    lock (this)
                    {
                        GameManager.Instance.AddGold((long)goldEarn);
                    }
                }).SetEase(Ease.Linear);
            }
            owner.SwitchState(DeliveryState.Return);
        }
    }
}

public class DeliveryReturn : IDeliveryState
{
    public void OnEnter(Delivery owner)
    {
        owner.anim.Play("Move");
        GameManager.Instance.market.currentDeliveries[owner.marketSlot] = null;
        owner.marketSlot = -1;
        owner.agent.SetDestination(GameManager.Instance.market.deliveryEnd.position);
    }

    public void OnExit(Delivery owner)
    {
    }

    public void OnUpdate(Delivery owner)
    {
        if (Vector3.Distance(owner.transform.position, GameManager.Instance.market.deliveryEnd.position) < 0.5f)
        {
            GameManager.Instance.activeDeliveries.Remove(owner);
            owner.production = null;
            owner.Recycle();
        }
    }
}