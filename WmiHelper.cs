using System.Management;

namespace IntelDSCEnable;

public static class WmiHelper
{
    public static string GetSignedDriverPropertyValue(string deviceName, string propertyName)
    {
        var mo = QueryMo($"SELECT ClassGuid FROM Win32_PnPSignedDriver WHERE DeviceName = '{deviceName}'").FirstOrDefault() ?? throw new Exception($"Can't find device with name: {deviceName}");
        return GetManagementPropertyValue(mo, propertyName) ?? throw new Exception($"Device found, but {propertyName} property is missing");
    }
    
    private static IEnumerable<ManagementBaseObject> QueryMo(string query)
    {
        using var driverSearcher = new ManagementObjectSearcher(query);
        using var em = driverSearcher.Get().GetEnumerator();
        while (em.MoveNext())
            yield return em.Current;
    }

    private static string? GetManagementPropertyValue(this ManagementBaseObject target, string propName)
    {
        try
        {
            return target[propName].ToString();
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.NotFound)
        {
            return null;
        }
    }
}