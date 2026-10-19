using Leopotam.EcsLite;

public class BusinessPresenter
{
    private readonly BusinessView _view;

    private readonly EcsPool<PurchaseRequest> _requestPool;
    private readonly EcsPool<BusinessComponent> _businessPool;
    private readonly BusinessData _businessData;
    private readonly BusinessNames _businessNames;

    private readonly int _entity;

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

    public void UpdateView()
    {
        ref BusinessComponent entityBusiness = ref _businessPool.Get(_entity);
        float firstUpgradeIncomeBoost = entityBusiness.FirstUpgradeBought ? _businessData.FirstUpgradeIncomeBoost : 0;
        float secondUpgradeIncomeBoost = entityBusiness.SecondUpgradeBought ? _businessData.SecondUpgradeIncomeBoost : 0;
        int level = entityBusiness.Level;
        float incomeProgress = entityBusiness.IncomeProgress / _businessData.IncomeLatency;
        int firstUpgradeBoost = (int)(_businessData.FirstUpgradeIncomeBoost * 100f);
        int secondUpgradeBoost = (int)(_businessData.SecondUpgradeIncomeBoost * 100f);
        _view.SetBusinessName(_businessNames.BusinessName);
        _view.SetLevel(level);
        _view.SetIncome(level* _businessData.BaseIncome * (1 + firstUpgradeIncomeBoost + secondUpgradeIncomeBoost));
        _view.SetNextLevelCost((level + 1) * _businessData.BaseCost);
        _view.SetIncomeProgress(incomeProgress);
        _view.SetFirstUpgradeInfo(_businessNames.FirstUpgradeName ,entityBusiness.FirstUpgradeBought, _businessData.FirstUpgradeBaseCost, firstUpgradeBoost);
        _view.SetSecondUpgradeInfo(_businessNames.SecondUpgradeName,entityBusiness.SecondUpgradeBought, _businessData.SecondUpgradeBaseCost, secondUpgradeBoost);
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
