using Refit;
using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Interfaces
{
    public interface IIdentityProvider
    {
        [Get("/User")]
        Task<IEnumerable<UserDto>> GetAllUser([Authorize("Bearer")] string token);

        [Get("/User/GetUserbyListId")]
        Task<IEnumerable<UserListDto>> GetUserbyListId(
            [Body] List<string> ListUserId, [Authorize("Bearer")] string token);

        //[Get("/User/GetUserbyListId")]
        //Task<IEnumerable<UserListDto>> GetUserbyListId(
        //    [Body] List<string> ListUserId, [HeaderCollection] IDictionary<string, string> headers);
    }
}
