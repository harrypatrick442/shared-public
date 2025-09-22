using Microsoft.Win32;

namespace Core.Registry
{
    public static class RegistryPaths
    {
        public const string ADINACAB_BASE_PATH = "Software\\Snippets";
        public const string AWS_UPLOADER_BASE_PATH = ADINACAB_BASE_PATH + "\\AWSUploader";
        private static readonly object _LockObjectGet = new object();


        private static readonly RegistryPath _AWSUploaderSelectedDirectory = new RegistryPath(RegistryHive.CurrentUser, AWS_UPLOADER_BASE_PATH, "SelectedDirectory");
        public static RegistryPath AWSUploaderSelectedDirectory { get { return _AWSUploaderSelectedDirectory; } }
        private static readonly RegistryPath _AWSUploaderAWSKeyId = new RegistryPath(RegistryHive.CurrentUser, AWS_UPLOADER_BASE_PATH, "AWSKeyId");
        public static RegistryPath AWSUploaderAWSKeyId { get { return _AWSUploaderAWSKeyId; } }
        private static readonly RegistryPath _AWSUploaderAWSBucket = new RegistryPath(RegistryHive.CurrentUser, AWS_UPLOADER_BASE_PATH, "AWSBucket");
        public static RegistryPath AWSUploaderAWSBucket { get { return _AWSUploaderAWSBucket; } }
        private static readonly RegistryPath _AWSUploaderAWSKeyPrefix = new RegistryPath(RegistryHive.CurrentUser, AWS_UPLOADER_BASE_PATH, "AWSKeyPrefix");
        public static RegistryPath AWSUploaderAWSKeyPrefix { get { return _AWSUploaderAWSKeyPrefix; } }
        private static readonly RegistryPath _AWSUploaderAWSSecretAccessKey = new RegistryPath(RegistryHive.CurrentUser, AWS_UPLOADER_BASE_PATH, "AWSSecretAccessKey");
        public static RegistryPath AWSUploaderAWSSecretAccessKey { get { return _AWSUploaderAWSSecretAccessKey; } }

    }
}
