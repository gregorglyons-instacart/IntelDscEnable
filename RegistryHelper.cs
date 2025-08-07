using Microsoft.Win32;

namespace IntelDSCEnable;

public static class RegistryHelper
{
    public static RegistryKey GetDriverKey(Guid classGuid)
    {
        var registryPath = $@"SYSTEM\CurrentControlSet\Control\Class\{classGuid:B}\0000\";
        return Registry.LocalMachine.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree) ?? throw new Exception($"Can't find registry path, looking at: {registryPath}");
    }

    public static DscMode ReadDscMode(this RegistryKey registryKey)
    {
        return (DscMode)(registryKey.GetValue("DPMstDscDisable") as int? ?? -1);
    }

    public static void SetDscMode(this RegistryKey registryKey, DscMode mode)
    {
        registryKey.SetValue("DPMstDscDisable", (int)mode);
    }
}