using System;
using System.Linq;
using UnityEngine;

public enum BuildingState
{
    PlaceHolder,
    Constructing,
    Producing,
}

public class Building : MonoBehaviour
{
    public BuildingInfo currentInfo { get; private set; }
    public ProductInfo productInfo;

    public BuildingState currentState { get; private set; }
    [SerializeField][LabeledArray(typeof(BuildingState))] private Animation[] gfxs;
    public Transform[] productPlaceHolders;
    public Transform buildFocus;
    public Transform deliveryPoint;
    public ParticleSystem buildDoneEff;

    ConstructionProgressUI progressUI;

    public int currentLevel;
    private float timer;

    public void Init(BuildingInfo baseInfo)
    {
        this.currentInfo = baseInfo;
        this.productInfo = GameManager.Instance.gameData.productData.First(x => x.id == currentInfo.productId);
        this.currentLevel = 0;
        this.currentState = BuildingState.PlaceHolder;
        OnEnter(this.currentState);
    }

    public void SwitchState(BuildingState nextState)
    {
        //Note: Finite state machine for easier management and worst performance
        if (nextState != currentState)
        {
            OnExit(this.currentState);

            this.currentState = nextState;

            OnEnter(this.currentState);
        }
    }

    void OnEnter(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.PlaceHolder:
                break;
            case BuildingState.Constructing:
                timer = currentInfo.buildTime;
                progressUI = GameManager.Instance.ingameUI.GetProgressUI(this);
                break;
            case BuildingState.Producing:
                buildDoneEff.Play();
                timer = currentInfo.productionTime;
                break;
        }

        for (int i = 0; i < gfxs.Length; ++i)
        {
            gfxs[i].gameObject.SetActive(i == (int)state);
        }
        gfxs[(int)state].Play();
    }

    void OnExit(BuildingState state)
    {
        switch (state)
        {
            case BuildingState.PlaceHolder:
                break;
            case BuildingState.Constructing:
                progressUI.Recycle();
                progressUI = null;
                break;
            case BuildingState.Producing:
                break;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case BuildingState.PlaceHolder:
                break;
            case BuildingState.Constructing:
                {
                    if (timer <= 0)
                    {
                        SwitchState(BuildingState.Producing);
                    }
                    else
                    {
                        progressUI.SetProgress(timer / currentInfo.buildTime, timer.ToString("F1"));
                    }
                }
                break;
            case BuildingState.Producing:
                if (timer <= 0)
                {
                    var index = Array.FindIndex(productPlaceHolders, 0, currentInfo.productCount, x => x.transform.childCount == 0);
                    if (index >= 0)
                    {
                        Instantiate(productInfo.productGFX, productPlaceHolders[index].transform);
                    }
                    timer = currentInfo.productionTime;
                }
                break;
        }

        timer -= Time.deltaTime;
    }

    void OnMouseUp()
    {
        switch (currentState)
        {
            case BuildingState.PlaceHolder:
                GameManager.Instance.ingameUI.buildingView.Init(this);
                break;
            case BuildingState.Constructing:
                break;
            case BuildingState.Producing:
                GameManager.Instance.ingameUI.upgradeView.Init(this);
                break;
        }
    }
}
