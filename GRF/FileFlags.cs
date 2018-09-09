using System;

namespace GRF
{
    [Flags]
    public enum FileFlag : byte
    {
        File = 1,
        MixedEncrypted = 2,
        DES = 4
    }
}
