using dockerapi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Linq;
using Server.Data;

namespace dockerapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
#pragma warning disable CS1591
    public class BlogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }
#pragma warning restore CS1591

        /// <summary>
        /// This method shows all blogs
        /// </summary>
        /// <returns></returns>
        ///<remarks>
        /// Sample request
        /// GET/Blog
        /// </remarks> 
        [HttpGet]
        public object Get()
        {
	        return _context.UserRoles.ToArray();
        }


    }
}