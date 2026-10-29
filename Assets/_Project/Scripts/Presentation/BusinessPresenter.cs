using Leopotam.EcsLite;

public class BusinessPresenter
{
    private readonly BusinessView _view;

    private readonly EcsPool<PurchaseRequest> _requestPool;
    private readonly EcsPool<BusinessComponent> _businessPool;
    private readonly BusinessData _businessData;
    private readonly BusinessNames _businessNames;

    private readonly int _entity;

    public int Entity => _entity;

    public BusinessPresenter(
    BusinessView view,
    int entity,
    EcsPool<PurchaseRequest> requestPool,
    EcsPool<BusinessComponent> businessPool,
    BusinessData businessData,
    BusinessNames businessNames)
    {
        _view = view;
        _entity = entity;
        _requestPool = requestPool;
        _businessPool = businessPool;
        _businessData = businessData;
        _businessNames = businessNames;
    }

    public void Subscribe()
    {
        _view.PurchaseButtonPressed += OnPurchaseButtonPressed;
        _view.FirstUpgradeButtonPressed += OnFirstUpgradeButtonPressed;
        _view.SecondUpgradeButtonPressed += OnSecondUpgradeButtonPressed;
    }

    public void Unsubscribe()
    {
        _view.PurchaseButtonPressed -= OnPurchaseButtonPressed;
        _view.FirstUpgradeButtonPressed -= OnFirstUpgradeButtonPressed;
        _view.SecondUpgradeButtonPressed -= OnSecondUpgradeButtonPressed;
    }

    public void InitializeView()
    {
        _view.SetBusinessName(_businessNames.BusinessName);
        UpdateBusinessInfo();
    }

    public void UpdateBusinessInfo()
    {
        ref BusinessComponent entityBusiness = ref _businessPool.Get(_entity);

        int firstUpgradeBoost = (int)(_businessData.FirstUpgradeIncomeBoost * 100f);
        int secondUpgradeBoost = (int)(_businessData.SecondUpgradeIncomeBoost * 100f);
        int level = entityBusiness.Level;

        _view.SetLevel(level);
        _view.SetIncome(BusinessCalculator.CalculateIncome(entityBusiness, _businessData));
        _view.SetNextLevelCost(BusinessCalculator.CalculateLevelCost(entityBusiness, _businessData));
        _view.SetFirstUpgradeInfo(_businessNames.FirstUpgradeName, entityBusiness.FirstUpgradeBought, _businessData.FirstUpgradeBaseCost, firstUpgradeBoost);
        _view.SetSecondUpgradeInfo(_businessNames.SecondUpgradeName, entityBusiness.SecondUpgradeBought, _businessData.SecondUpgradeBaseCost, secondUpgradeBoost);
    }

    public void UpdateProgress()
    {
        ref BusinessComponent entityBusiness = ref _businessPool.Get(_entity);

        float incomeProgress = entityBusiness.IncomeProgress / _businessData.IncomeLatency;

        _view.SetIncomeProgress(incomeProgress);
    }
    private void OnSecondUpgradeButtonPressed()
    {
        if (!_requestPool.Has(_entity))
        {
            _requestPool.Add(_entity);
            _requestPool.Get(_entity).Type = PurchaseType.SecondUpgrade;
        }
    }

    private void OnFirstUpgradeButtonPressed()
    {
        if (!_requestPool.Has(_entity))
        {
            _requestPool.Add(_entity);
            _requestPool.Get(_entity).Type = PurchaseType.FirstUpgrade;
        }
    }

    private void OnPurchaseButtonPressed()
    {
        if (!_requestPool.Has(_entity))
        {
            _requestPool.Add(_entity);
            _requestPool.Get(_entity).Type = PurchaseType.Level;
        }
    }
}
