using System;
namespace Logging
{
    public static class BreadcrumbCategoryNameExtensions
    {
        public static string GetString(this BreadcrumbCategoryName breadcrumbCategoryName)
        {
            switch (breadcrumbCategoryName)
            {
                case BreadcrumbCategoryName.Check:
                    return "CHECK";
                case BreadcrumbCategoryName.Fix:
                    return "FIX";
                case BreadcrumbCategoryName.Main:
                    return "Main";
                case BreadcrumbCategoryName.Util:
                    return "Util";
                case BreadcrumbCategoryName.Power:
                    return "Power";
                case BreadcrumbCategoryName.Logger:
                    return "LOGGER";
                case BreadcrumbCategoryName.Installer:
                    return "Installer";
            }
            return Enum.GetName(typeof(BreadcrumbCategoryName), breadcrumbCategoryName);
        }
    }
}

