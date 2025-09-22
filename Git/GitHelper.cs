using LibGit2Sharp;
namespace Setup.Git
{
    public static class GitHelper
    {
        public static void Clone(string gitRepositoryUrl, string directoryPath, 
            string username, string password)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
                CloneOptions cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider = (_url, _user, _cred) => 
                new UsernamePasswordCredentials { Username = username,
                    Password = password };
                Repository.Clone(gitRepositoryUrl, directoryPath, cloneOptions);
            }
            catch (Exception ex) {
                throw new Exception($"Failed while trying to clone \"{gitRepositoryUrl}\" to directory \"{directoryPath}\"", ex);
            }
        }
    }
}