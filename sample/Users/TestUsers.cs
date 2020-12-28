using Glow.Users;

namespace Glow.Sample.Users
{
    public class TestUsers
    {
        public static UserDto TestUser()
        {
            return new UserDto
            {
                Id = "1",
                DisplayName = "Stanley Kubrick",
                Email = "sku@qna.com"
            };
        }

        //public static UserDto NotAuthorizedUser()
        //{

        //}

        public static UserDto NonPrivilegedUser()
        {
            return new UserDto
            {
                Id = "2",
                DisplayName = "Christopher Nolan",
                Email = "cno@qna.com"
            };
        }
    }
}
