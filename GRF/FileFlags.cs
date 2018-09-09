using System;

namespace GRF
{
    [Flags]
    public enum FileFlag : byte
    {
        File = 1,
        Mixcrypt = 2,
        DES = 4
    }
}
