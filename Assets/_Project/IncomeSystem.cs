using Leopotam.EcsLite;
using UnityEngine;

public class IncomeSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly BusinessConfig _businessConfig;
    private EcsPool<BusinessComponent> _businessPool;
    private EcsPool<BalanceComponent> _balancePool;
    private EcsFilter _filter;
    private int _balanceId;
    public IncomeSystem(BusinessConfig businessConfig, int balanceId)
    {
        _businessConfig = businessConfig;
        _balanceId = balanceId;
    }

    public void Init(IEcsSystems systems)
    {
        EcsWorld world = systems.GetWorld();
        _businessPool = world.GetPool<BusinessComponent>();
        _balancePool = world.GetPool<BalanceComponent>();
        _filter = world.Filter<BusinessComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (int entity in _filter)
        {
            ref BusinessComponent entityBusiness = ref _businessPool.Get(entity);
            int businessId = entityBusiness.BusinessId;
            BusinessData data = _businessConfig.Businesses[businessId];

            if (entityBusiness.Level != 0)
            {
                entityBusiness.IncomeProgress += Time.deltaTime;
            }
            else
            {
                continue;
            }

            float firstUpgradeIncomeBoost = entityBusiness.FirstUpgradeBought ? data.FirstUpgradeIncomeBoost : 0;
            float secondUpgradeIncomeBoost = entityBusiness.SecondUpgradeBought? data.SecondUpgradeIncomeBoost : 0;

            ref BalanceComponent _balance = ref _balancePool.Get(_balanceId);
            while (entityBusiness.IncomeProgress >= data.IncomeLatency)
            {
                entityBusiness.IncomeProgress -= data.IncomeLatency;
                _balance.Value += entityBusiness.Level * data.BaseIncome * (1f + firstUpgradeIncomeBoost + secondUpgradeIncomeBoost);
                Debug.Log($"Balance: {_balance.Value}");
            }
        }
    }
}
