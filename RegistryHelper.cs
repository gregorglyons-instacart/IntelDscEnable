using Microsoft.Win32;

namespace IntelDSCEnable;

public static class RegistryHelper
{
    public static RegistryKey GetDriverKey(string deviceId)
    {
        using var enumKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{deviceId}");
        var driver = enumKey?.GetValue("Driver") as string ??  throw new Exception($"Can't find driver with name: {deviceId}");
        
        var registryPath = $@"SYSTEM\CurrentControlSet\Control\Class\{driver}";
        return Registry.LocalMachine.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree) ?? throw new Exception($"Can't find registry path, looking at: {registryPath}");
    }

    extension(RegistryKey registryKey)
    {
        public DscMode ReadDscMode()
        {
            return (DscMode)(registryKey.GetValue("DPMstDscDisable") as int? ?? -1);
        }

        public void SetDscMode(DscMode mode)
        {
            registryKey.SetValue("DPMstDscDisable", (int)mode);
        }
    }
}