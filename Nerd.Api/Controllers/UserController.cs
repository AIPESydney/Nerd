using Microsoft.AspNet.Identity.Owin;
using Nerd.Api.Cache;
using Nerd.Api.Models;
using Nerd.Api.Repository;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace Nerd.Api.Controllers
{

    [Authorize]
    public class UserController : ApiController
    {
        
        [Inject]
        public ICacheManager CacheManager { get; set; }
        private readonly UnitOfWork _unitOfWork;
        private ApplicationUserManager _userManager;
        public UserController(UnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException("unitOfWork");

            this._unitOfWork = unitOfWork;
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return _userManager ?? (_userManager = ApplicationUserManager.Create(
                    new IdentityFactoryOptions<ApplicationUserManager>()
                    {
                        Provider = new IdentityFactoryProvider<ApplicationUserManager>()
                    }, Request.GetOwinContext()));
            }
            private set
            {
                _userManager = value;
            }
        }


        [HttpGet]
        public async Task<IHttpActionResult> Get(int userId)
        {
            await Task.Delay(1);
            return Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post(RegisterBindingModel model)
        {
            await Task.Delay(1);
            return Ok();
        }
        [System.Web.Http.Route("api/user/getusers")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUsers()
        {
            await Task.Delay(1);
            var users = _unitOfWork.UserRepository.GetAll();
            return Ok(users);
        }


        //Paging concepts
        [System.Web.Http.Route("api/user/getusersp")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUsersP(int pageSize, int pageNumber)
        {
            try
            {
                var totalCount = await _unitOfWork.UserRepository.CountAsync(c => c.IsActive);
                var totalPages = Math.Ceiling((double)totalCount / pageSize);
                var query = _unitOfWork.UserRepository.SearchFor(c => c.IsActive).OrderBy(c => c.Id);
                var user = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var result = new
                {
                    TotalCount = totalCount,
                    TotalPage = totalPages,
                    Users = user
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                //Log exception
                return InternalServerError(ex);
            }




        }
    }
}