using System.Collections.Generic;
using UnityEngine;

public static class GameConfigValidator 
{

    public static bool AreIdsMatching(Dictionary<int, BusinessView> viewsById, Dictionary<int, BusinessData> businessDataById, Dictionary<int, BusinessNames> businessNamesById)
    {

        foreach (int businessId in businessDataById.Keys)
        {
            if (!viewsById.ContainsKey(businessId) ||
                !businessNamesById.ContainsKey(businessId))
            {
                Debug.LogError($"Missing data for BusinessId: {businessId}");
                return false;
            }
        }
        return true;
    }

    public static bool IsNameValid(BusinessNamesConfig businessNames)
    {
        HashSet<int> ids = new();

        foreach (BusinessNames name in businessNames.BusinessNames)
        {
            if (name == null)
            {
                Debug.LogError("BusinessName is missing.");
                return false;
            }
            if (!ids.Add(name.BusinessId))
            {
                Debug.LogError($"Duplicate BusinessId: {name.BusinessId}");
                return false;
            }
        }
        return true;
    }

    public static bool IsDataValid(BusinessConfig businessConfig)
    {
        HashSet<int> ids = new();

        foreach (BusinessData data in businessConfig.Businesses)
        {
            if (data == null)
            {
                Debug.LogError("BusinessData is missing.");
                return false;
            }

            if (!ids.Add(data.BusinessId))
            {
                Debug.LogError($"Duplicate BusinessId: {data.BusinessId}");
                return false;
            }

            if (data.IncomeLatency <= 0)
            {
                Debug.LogError($"Invalid IncomeLatency for BusinessId: {data.BusinessId}");
                return false;
            }
        }

        return true;
    }

    public static bool IsViewValid(BusinessView[] views)
    {
        HashSet<int> ids = new();

        foreach (BusinessView businessView in views)
        {
            if (businessView == null)
            {
                Debug.LogError("BusinessView is missing.");
                return false;
            }
            if (!ids.Add(businessView.BusinessId))
            {
                Debug.LogError($"Duplicate BusinessId: {businessView.BusinessId}");
                return false;
            }
        }
        return true;
    }

    public static bool IsConfigurationValid(BusinessConfig businessConfig, BusinessNamesConfig namesConfig, BalanceView balanceView, BusinessView[] views)
    {
        if (businessConfig == null ||
    businessConfig.Businesses == null ||
    namesConfig == null ||
    namesConfig.BusinessNames == null ||
    balanceView == null ||
    views == null ||
    views.Length != businessConfig.Businesses.Length ||
    namesConfig.BusinessNames.Length != businessConfig.Businesses.Length)
        {
            Debug.LogError("Invalid game configuration.");
            return false;
        }

        return true;
    }
}
