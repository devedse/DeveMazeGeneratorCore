using System.ComponentModel;

namespace DeveMazeGenerator.Generators
{
    public enum InnerMapType
    {
        [Description("Uses n/8 bytes of memory, pretty fast, no multithreading")]
        BitArreintjeFast,

        [Description("Uses n bytes of memory, really fast, multithreading")]
        BooleanArray,

        [Description("Uses n/8 bytes of memory, average speed, no multithreading")]
        DotNetBitArray,

        [Description("Uses 0 bytes of memory, super slow, god knows what happens when multithreading")]
        BitArrayMappedOnHardDisk,

        [Description("Hybrid map")]
        Hybrid
    }
}
