using Leopotam.EcsLite;
using System.Collections.Generic;
using UnityEngine;

public class IncomeSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly Dictionary<int, BusinessData> _businessDataById;
    private EcsPool<BusinessComponent> _businessPool;
    private EcsPool<BalanceComponent> _balancePool;
    private EcsPool<BalanceChanged> _balanceChangedPool;
    private EcsFilter _filter;
    private int _balanceId;
    public IncomeSystem(Dictionary<int, BusinessData> businessDataById, int balanceId)
    {
        _businessDataById = businessDataById;
        _balanceId = balanceId;
    }

    public void Init(IEcsSystems systems)
    {
        EcsWorld world = systems.GetWorld();
        _businessPool = world.GetPool<BusinessComponent>();
        _balancePool = world.GetPool<BalanceComponent>();
        _filter = world.Filter<BusinessComponent>().End();
        _balanceChangedPool = world.GetPool<BalanceChanged>();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (int entity in _filter)
        {
            ref BusinessComponent entityBusiness = ref _businessPool.Get(entity);
            int businessId = entityBusiness.BusinessId;
            BusinessData data = _businessDataById[businessId];

            if (entityBusiness.Level != 0)
            {
                entityBusiness.IncomeProgress += Time.deltaTime;
            }
            else
            {
                continue;
            }

            ref BalanceComponent _balance = ref _balancePool.Get(_balanceId);
            while (entityBusiness.IncomeProgress >= data.IncomeLatency)
            {
                entityBusiness.IncomeProgress -= data.IncomeLatency;
                _balance.Value += BusinessCalculator.CalculateIncome(entityBusiness, data);
                Debug.Log("Balance: " + _balance.Value);
                if (!_balanceChangedPool.Has(_balanceId))
                {
                    _balanceChangedPool.Add(_balanceId);
                }
            }
        }
    }
}
