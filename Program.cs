using IntelDSCEnable;

var deviceId = WmiHelper.GetSignedDriverPropertyValue("Intel(R) Iris(R) Xe Graphics", "DeviceID");

var driverKeys = RegistryHelper.GetDriverKey(deviceId);

var originalMode = driverKeys.ReadDscMode();

Console.WriteLine(driverKeys.Name);

if (originalMode != DscMode.Enabled)
{
    driverKeys.SetDscMode(DscMode.Enabled);
    Console.WriteLine("DSC set. Reboot required.");
}
else
{
    Console.WriteLine("DSC is already enabled.");
}

var limitedMonitors = User32.GetMonitorDetails().Where(itm => itm.Is4K() && itm.RefreshRate == 24).ToArray();
if (limitedMonitors.Length > 0)
{
    Console.WriteLine("Warning: Some of your monitors look like they are currently bandwidth limited (4k @24hz, instead of 60hz)");
}