namespace IntelDSCEnable;

public readonly record struct MonitorDetail(string Name, int Width, int Height, int RefreshRate, int BitsPerPixel, int PositionX, int PositionY)
{
    public bool Is4K()
    {
        return (Width == 2160 && Height == 3840) || Width == 3840 && Height == 2160;
    }
}