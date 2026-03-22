namespace SkyLabIdP.Domain.Enums;
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1 << 0,        // 1
    Search = 1 << 1,      // 2
    Create = 1 << 2,      // 4
    Update = 1 << 3,      // 8
    Delete = 1 << 4,      // 16
    Upload = 1 << 5,      // 32
    Download = 1 << 6,    // 64
    Import = 1 << 7,      // 128
    Export = 1 << 8       // 256
}
