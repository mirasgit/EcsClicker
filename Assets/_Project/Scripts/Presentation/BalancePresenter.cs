using Leopotam.EcsLite;

public class BalancePresenter
{
    private BalanceView _balanceView;
    private EcsPool<BalanceComponent> _balancePool;

    private int _entity;

    public BalancePresenter(BalanceView balanceView, int entity, EcsPool<BalanceComponent> balancePool)
    {
        _balanceView = balanceView;
        _entity = entity;
        _balancePool = balancePool;
    }

    public void UpdateBalance()
    {
        ref BalanceComponent balance = ref _balancePool.Get(_entity);
        _balanceView.SetBalance(balance.Value);
    }
}
