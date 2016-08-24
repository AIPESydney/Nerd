using Microsoft.AspNet.Identity.Owin;
using Nerd.Api.Models;
using Nerd.Api.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace Nerd.Api.Controllers
{
    public class UserController : ApiController
    {
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

        [HttpGet]
        public async Task<IHttpActionResult> GetUsers()
        {
            await Task.Delay(1);
            var users = _unitOfWork.UserRepository.GetAll();
            return Ok(users);
        }
    }
}