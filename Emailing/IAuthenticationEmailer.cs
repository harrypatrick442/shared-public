namespace Emailing
{
    public interface IAuthenticationEmailer
    {
        public void SendPasswordResetEmail(string email, string username,
            string operatingSystem, string browserName,
            string actionUrl);
    }
}