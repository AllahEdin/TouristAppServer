using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class TestController : BaseController
    {
        [HttpGet]
		[Authorize]
        public string Test()
        {
            return "succeed get";
        }
    }
}