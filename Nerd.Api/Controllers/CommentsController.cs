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

    public class CommentsController : ApiController
    {
        [Inject]
        public ICacheManager CacheManager { get; set; }
        private readonly UnitOfWork _unitOfWork;
        private ApplicationUserManager _userManager;
        public CommentsController(UnitOfWork unitOfWork)
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

        [System.Web.Http.Route("api/comments/getuserposts")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUserPosts(int userId) {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var comments = await _unitOfWork.CommentsRepository.GetAsync(c => c.UserId == userId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}