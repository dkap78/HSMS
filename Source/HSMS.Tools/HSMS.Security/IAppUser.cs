using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSMS.Security
{
    public interface IAppUser
    {
        #region Propreties
        Guid Id { get; set; }
        string? FirstName { get; set; }
        string? LastName { get; set; }
        string? Phone { get; set; }
        string Password { get; set; }
        string Email { get; set; }
        bool IsActive { get; set; }
        #endregion
    }
    public interface IChangePassword : IAppUser
    {
        #region Properties
        string NewPassword { get; set; }
        #endregion
    }
}
