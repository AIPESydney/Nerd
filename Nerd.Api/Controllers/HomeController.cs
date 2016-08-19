using Nerd.Api.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nerd.Api.Controllers
{
    public class HomeController : Controller
    {
        private UnitOfWork _unitOfWork;

        public HomeController()
        {
            _unitOfWork = new UnitOfWork();
        }



        public ActionResult Index()
        {

            //Id
            int userid=123;
            var user = _unitOfWork.UserRepository.Get(u => u.Id == userid);

            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
