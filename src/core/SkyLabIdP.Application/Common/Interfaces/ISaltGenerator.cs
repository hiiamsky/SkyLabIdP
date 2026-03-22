namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface ISaltGenerator
    {
        string GenerateSecureSalt(int size = 16);
    }

}

