using System;


namespace KeyValuePairDatabases
{
    public interface IManagedMemoryUse
    {
        long BytesMemoryAtDisposal { set; }
    }
}