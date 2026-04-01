using System.Collections.Generic;
using System.Linq;

public class UnitRegister
{
    public static readonly Dictionary<int, UnitAgentData> unitRegistry = new();
    private static int idCounter = 0;

    public static int Register(UnitAgentData unitData)
    {
        if (unitRegistry.ContainsValue(unitData))
        {
            return unitData.unitID;
        }
        else
        {
            unitRegistry[idCounter++] = unitData;
            return idCounter - 1;
        }
    }

    public static void Unregister(int unitID)
    {
        unitRegistry.Remove(unitID);
    }
}
