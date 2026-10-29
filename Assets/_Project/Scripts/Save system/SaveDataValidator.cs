using System.Collections.Generic;

public static class SaveDataValidator
{
    public static bool IsValid(
        GameSaveData saveData,
        Dictionary<int, BusinessData> businessDataById)
    {
        if (saveData == null ||
            businessDataById == null ||
            saveData.BusinessDataSaves == null ||
            saveData.BusinessDataSaves.Length != businessDataById.Count ||
            float.IsNaN(saveData.Balance) ||
            float.IsInfinity(saveData.Balance) ||
            saveData.Balance < 0)
        {
            return false;
        }

        HashSet<int> encounteredIds = new HashSet<int>();

        foreach (BusinessSaveData business in saveData.BusinessDataSaves)
        {
            if (business == null || 
                !businessDataById.TryGetValue(business.BusinessId, out BusinessData config) || 
                !encounteredIds.Add(business.BusinessId))
            {
                return false;
            }

            if (business.BusinessLevel < 0 ||
                    business.IncomeProgress < 0 ||
                    float.IsNaN(business.IncomeProgress) ||
                    float.IsInfinity(business.IncomeProgress) ||
                    business.IncomeProgress >= config.IncomeLatency)
            {
                return false;
            }
        }

        return true;
    }
}