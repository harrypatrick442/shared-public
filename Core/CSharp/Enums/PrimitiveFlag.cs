using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Core.Enums
{
    [Flags]
    public enum PrimitiveFlag
    {
        None=0,
        BuiltIntoSoftware=1,
        Flag=2,
        Project=4,
        CreateCanSeeConnectionToEveryUserByDefault = 8,
        CreateCanManageOwnConnectionToEveryUserByDefault = 16

    }
    public static class PrimitiveFlags {
        public static readonly PrimitiveFlag MeansDoSomethingToEveryNewUser = 
             PrimitiveFlag.CreateCanSeeConnectionToEveryUserByDefault |
            PrimitiveFlag.CreateCanManageOwnConnectionToEveryUserByDefault;

    }
}

