using HSMS.DB.Models;

namespace HSMS.API
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User u) => new()
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Phone = u.Phone
        };
    }

}
