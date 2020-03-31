namespace JannikB.Glue.AspNetCore.Tests
{

    public class UserDto : IUser
    {
        [RequiredIfNullOrEmpty("DisplayName", "Email")]
        public string Id { get; set; }
        [RequiredIfNullOrEmpty("Id")]
        public string DisplayName { get; set; }
        [RequiredIfNullOrEmpty("Id")]
        [EmailIfNullOrEmpty("Id")]
        public string Email { get; set; }
    }
}
