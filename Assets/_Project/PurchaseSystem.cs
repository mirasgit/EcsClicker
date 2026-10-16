using Leopotam.EcsLite;
using System.Collections.Generic;

public class PurchaseSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly BusinessConfig _businessConfig;
    private readonly int _balanceId;
    private readonly List<int> _processedPurchases = new();

    private EcsPool<BusinessComponent> _businessPool;
    private EcsPool<PurchaseRequest> _requestPool;
    private EcsPool<BalanceComponent> _balancePool;
    private EcsFilter _filter;
    public PurchaseSystem(BusinessConfig businessConfig, int balanceId)
    {
        _businessConfig = businessConfig;
        _balanceId = balanceId;
    }

    public void Init(IEcsSystems systems)
    {
        EcsWorld world = systems.GetWorld();
        _businessPool = world.GetPool<BusinessComponent>();
        _balancePool = world.GetPool<BalanceComponent>();
        _filter = world.Filter<BusinessComponent>().Inc<PurchaseRequest>().End();
        _requestPool = world.GetPool<PurchaseRequest>();
        
    }

    public void Run(IEcsSystems systems)
    {
        ref BalanceComponent balance = ref _balancePool.Get(_balanceId);
        foreach (int entity in _filter)
        {
            _processedPurchases.Add(entity);
            ref BusinessComponent business = ref _businessPool.Get(entity);
            int businessId = business.BusinessId;
            BusinessData data = _businessConfig.Businesses[businessId];
                
            switch (_requestPool.Get(entity).Type)
            {
                case PurchaseType.Level:
                    int levelCost = (business.Level + 1) * data.BaseCost;
                    if (balance.Value >= levelCost)
                    {
                        balance.Value -= levelCost;
                        business.Level++;
                    }
                    break;

                case PurchaseType.FirstUpgrade:
                    int firstUpgradeCost = data.FirstUpgradeBaseCost;
                    if (business.Level > 0 && balance.Value >= firstUpgradeCost && !business.FirstUpgradeBought)
                    {
                        balance.Value -= firstUpgradeCost;
                        business.FirstUpgradeBought = true;
                    }
                    break;

                case PurchaseType.SecondUpgrade:
                    int secondUpgradeCost = data.SecondUpgradeBaseCost;
                    if (business.Level > 0 && balance.Value >= secondUpgradeCost && !business.SecondUpgradeBought)
                    {
                        balance.Value -= secondUpgradeCost;
                        business.SecondUpgradeBought = true;
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

}
