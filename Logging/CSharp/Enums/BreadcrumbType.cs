namespace Core.Enums
{
    public enum BreadcrumbType
    {
        UnknownOrNone = 0,
        Test = 1,
        MapSender=2,
        StartDownload=3,
        ClickedDownload=4,
        GotMapSenderResponse=5,
        GotStartDownloadResponse=6,
        GotNewToken= 7,
        UserAgent=8,
        Url = 9,
        Info=10,
        Warning=11
    }
    public static class BreadcrumbTypeHelper{
        public static BreadcrumbType Parse(int breadcrumbTypeId) {
            if (!Enum.IsDefined(typeof(BreadcrumbType), breadcrumbTypeId))
                return BreadcrumbType.UnknownOrNone;
            return (BreadcrumbType)breadcrumbTypeId;
        }
    }
}