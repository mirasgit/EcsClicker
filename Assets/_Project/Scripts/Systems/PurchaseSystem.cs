using Leopotam.EcsLite;
using System.Collections.Generic;

public class PurchaseSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly Dictionary<int, BusinessData> _businessDataById;
    private readonly int _balanceId;
    private readonly List<int> _processedPurchases = new();

    private EcsPool<BusinessComponent> _businessPool;
    private EcsPool<PurchaseRequest> _requestPool;
    private EcsPool<BalanceComponent> _balancePool;
    private EcsPool<BusinessChanged> _businessChangedPool;
    private EcsPool<BalanceChanged> _balanceChangedPool;
    private EcsFilter _filter;
    public PurchaseSystem(Dictionary<int, BusinessData> businessDataById, int balanceId)
    {
        _businessDataById = businessDataById;
        _balanceId = balanceId;
    }

    public void Init(IEcsSystems systems)
    {
        EcsWorld world = systems.GetWorld();
        _businessPool = world.GetPool<BusinessComponent>();
        _balancePool = world.GetPool<BalanceComponent>();
        _filter = world.Filter<BusinessComponent>().Inc<PurchaseRequest>().End();
        _requestPool = world.GetPool<PurchaseRequest>();
        _businessChangedPool = world.GetPool<BusinessChanged>();
        _balanceChangedPool = world.GetPool<BalanceChanged>();
    }

    public void Run(IEcsSystems systems)
    {
        ref BalanceComponent balance = ref _balancePool.Get(_balanceId);
        foreach (int entity in _filter)
        {
            _processedPurchases.Add(entity);
            ref BusinessComponent business = ref _businessPool.Get(entity);
            int businessId = business.BusinessId;
            BusinessData data = _businessDataById[businessId];
                
            switch (_requestPool.Get(entity).Type)
            {
                case PurchaseType.Level:
                    int levelcost = BusinessCalculator.CalculateLevelCost(business, data);
                    if (balance.Value >= levelcost)
                    {
                        balance.Value -= levelcost;
                        business.Level++;

                        MarkPurchaseChanges(entity);
                    }
                    break;

                case PurchaseType.FirstUpgrade:
                    int firstUpgradeCost = data.FirstUpgradeBaseCost;
                    if (business.Level > 0 && balance.Value >= firstUpgradeCost && !business.FirstUpgradeBought)
                    {
                        balance.Value -= firstUpgradeCost;
                        business.FirstUpgradeBought = true;

                        MarkPurchaseChanges(entity);
                    }
                    break;

                case PurchaseType.SecondUpgrade:
                    int secondUpgradeCost = data.SecondUpgradeBaseCost;
                    if (business.Level > 0 && balance.Value >= secondUpgradeCost && !business.SecondUpgradeBought)
                    {
                        balance.Value -= secondUpgradeCost;
                        business.SecondUpgradeBought = true;

                        MarkPurchaseChanges(entity);
                    }
                    break;

                default: 
                    break;
            }

        }
        foreach (int entity in _processedPurchases)
        {
            _requestPool.Del(entity);
        }
        _processedPurchases.Clear();
    }

    private void MarkPurchaseChanges(int entity)
    {
        if (!_balanceChangedPool.Has(_balanceId))
        {
            _balanceChangedPool.Add(_balanceId);
        }
        if (!_businessChangedPool.Has(entity))
        {
            _businessChangedPool.Add(entity);
        }
    }

}
