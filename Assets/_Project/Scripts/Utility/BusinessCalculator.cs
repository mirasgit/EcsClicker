public static class BusinessCalculator 
{
    public static float CalculateIncome(BusinessComponent component, BusinessData data)
    {
        float firstBoost = component.FirstUpgradeBought ? data.FirstUpgradeIncomeBoost : 0;
        float secondBoost = component.SecondUpgradeBought ? data.SecondUpgradeIncomeBoost: 0;
        return component.Level * data.BaseIncome * (1 + firstBoost + secondBoost);
    }

    public static int CalculateLevelCost(BusinessComponent component, BusinessData data)
    {
        return (component.Level+1) * data.BaseCost;
    }
}
