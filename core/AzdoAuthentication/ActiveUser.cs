namespace Glow.AzdoAuthentication
{
    public class ActiveUser
    {
        public ActiveUser(string id, string displayName, string email, string token)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            AccessToken = token;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string AccessToken { get; }
    }
}
