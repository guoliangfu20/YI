using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using YI.Core.CacheManager.IService;
using YI.Core.EFDbContext;
using YI.Core.Extensions;
using YI.Core.Extensions.AutofacManager;
using YI.Entity.DomainModel.System;
using YI.Entity.SystemModels;

namespace YI.Core.UserManager
{
    public class UserContext
    {
        /// <summary>
        /// 为了尽量减少redis或Memory读取,保证执行效率,将UserContext注入到DI，
        /// 每个UserContext的属性至多读取一次redis或Memory缓存从而提高查询效率
        /// </summary>
        public static UserContext Current
        {
            get
            {
                return Context.RequestServices.GetService(typeof(UserContext)) as UserContext;
            }
        }

        private static Microsoft.AspNetCore.Http.HttpContext Context
        {
            get
            {
                return Utilities.HttpContext.Current;
            }
        }

        private static ICacheService CacheService
        {
            get { return GetService<ICacheService>(); }
        }

        private static T GetService<T>() where T : class
        {
            return AutofacContainerModule.GetService<T>();
        }

        private UserInfo _userInfo { get; set; }
        public int UserId
        {
            get
            {
                return (Context.User.FindFirstValue(JwtRegisteredClaimNames.Jti)
                    ?? Context.User.FindFirstValue(ClaimTypes.NameIdentifier)).GetInt();
            }
        }

        public UserInfo UserInfo
        {
            get
            {
                if (_userInfo != null)
                {
                    return _userInfo;
                }
                return GetUserInfo(UserId);
            }
        }

        public UserInfo GetUserInfo(int userId)
        {
            if (_userInfo != null) return _userInfo;
            if (userId <= 0)
            {
                _userInfo = new UserInfo();
                return _userInfo;
            }
            string key = userId.GetUserIdKey();
            _userInfo = CacheService.Get<UserInfo>(key);
            if (_userInfo != null && _userInfo.User_Id > 0) return _userInfo;

            _userInfo = DBServerProvider.DbContext.Set<Sys_User>()
                .Where(x => x.User_Id == userId).Select(s => new
                {
                    User_Id = userId,
                    Role_Id = s.Role_Id.GetInt(),
                    RoleName = s.RoleName,
                    DeptId = s.Dept_Id ?? 0,
                    Token = s.Token,
                    UserName = s.UserName,
                    UserTrueName = s.UserTrueName,
                    Enable = s.Enable,
                    DeptIds = s.DeptIds
                }).ToList().Select(s => new UserInfo()
                {
                    User_Id = userId,
                    Role_Id = s.Role_Id,
                    Token = s.Token,
                    UserName = s.UserName,
                    UserTrueName = s.UserTrueName,
                    Enable = 1,
                    DeptIds = string.IsNullOrEmpty(s.DeptIds) ? new List<Guid>() : s.DeptIds.Split(",").Select(x => (Guid)x.GetGuid()).ToList(),
                }).FirstOrDefault();

            if (_userInfo != null && _userInfo.User_Id > 0)
            {
                CacheService.AddObject(key, _userInfo);
            }
            return _userInfo ?? new UserInfo();
        }

    }
}
