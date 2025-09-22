using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Core.Enums
{
    [Flags]
    public enum SnippetConnectionFlag
    {
        None=0,
        Browsing=1,//As user goes from A to B and closes A removed from B.
        CanSee=2,//Connection from A to B can be seen by user
        CanCreateConnections=4,
        CanModifyConnections = 8,
        CanDeleteConnections = 16,
        CanModifySnippet = 32,
        CanDeleteSnippet = 64,
        CanManageOwnConnection=128,
        ToType= 256,
        TypeOf=512,
        ToField=1024,
        FieldOf=2048,
        Partial=4096
    }
    public static class SnippetConnectionFlags {
        public static readonly SnippetConnectionFlag PermissionsFlags = (SnippetConnectionFlag.CanSee|SnippetConnectionFlag.CanModifyConnections | SnippetConnectionFlag.CanDeleteConnections|SnippetConnectionFlag.CanModifySnippet|SnippetConnectionFlag.CanDeleteSnippet|SnippetConnectionFlag.CanManageOwnConnection| SnippetConnectionFlag.CanCreateConnections);






        //public static readonly SnippetConnectionFlag OwnsMe = PermissionsFlags;

        //public static readonly SnippetConnectionFlag  AnyKindOfConnectionModification = (SnippetConnectionFlag.CanModifyConnections | SnippetConnectionFlag.CanDeleteConnections);
        private static readonly SnippetConnectionFlag[] AllFlags_Seperated = Enum.GetValues(typeof(SnippetConnectionFlag)).Cast<SnippetConnectionFlag>().ToArray();
        public static readonly SnippetConnectionFlag AllFlags = (SnippetConnectionFlag)AllFlags_Seperated.Sum(f => (int)f);
        public readonly static SnippetConnectionFlag FilterToAllNonPermissionFlags = (SnippetConnectionFlag)AllFlags_Seperated.Where(f => (f & PermissionsFlags) <= 0).Sum(f => (int)f);
        public readonly static SnippetConnectionFlag ForNewSnippet = PermissionsFlags;
        public readonly static SnippetConnectionFlag SafePermissionsToGiveOnUserSnippet = SnippetConnectionFlag.CanSee;
        public readonly static SnippetConnectionFlag ForNewFieldSnippetToSnippetOn = SnippetConnectionFlag.FieldOf | PermissionsFlags;
        public readonly static SnippetConnectionFlag ForNewFieldSnippetToTypeSnippet = SnippetConnectionFlag.ToType | SnippetConnectionFlag.CanSee;
        public readonly static SnippetConnectionFlag ToNewFieldSnippet = SnippetConnectionFlag.ToField | SnippetConnectionFlag.CanSee;
        public static SnippetConnectionFlag GetMissingFlags(SnippetConnectionFlag missingIn, SnippetConnectionFlag on) {
            return (missingIn ^ (on & missingIn));
        }
        public static bool HasFlag(this SnippetConnectionFlag flags, SnippetConnectionFlag theFlagItMayHave) {
            return (flags & theFlagItMayHave)== theFlagItMayHave;        }
    }
}

