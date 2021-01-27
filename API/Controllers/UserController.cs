using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using API.Models;
using Application.Exceptions;
using Domain;
using EFData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using Services.Interfaces;

namespace API.Controllers
{
    [AllowAnonymous]
    public class UserController : BaseController
    {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IJwtGenerator _jwtGenerator;
		private readonly DataContext _context;

		public UserController(IJwtGenerator jwtGenerator, 
			SignInManager<AppUser> signInManager, 
			UserManager<AppUser> userManager, 
			DataContext context)
		{
			_jwtGenerator = jwtGenerator;
			_signInManager = signInManager;
			_userManager = userManager;
			_context = context;
		}

		[SwaggerResponse(typeof(UserModel))]
		[HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginUserModel model)
        {
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				throw new RestException(HttpStatusCode.Unauthorized);
			}

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

			if (result.Succeeded)
			{
				return Ok(new UserModel
				{
					DisplayName = user.DisplayName,
					Token = _jwtGenerator.CreateToken(user),
					UserName = user.UserName,
					Image = null
				});
			}

			throw new RestException(HttpStatusCode.Unauthorized);
		}


		[SwaggerResponse(typeof(UserModel))]
		[HttpPost("registration")]
		public async Task<IActionResult> RegistrationAsync([FromBody] RegistrationUserModel model)
		{
			if (await _context.Users.Where(x => x.Email == model.Email).AnyAsync())
			{
				throw new RestException(HttpStatusCode.BadRequest, new { Email = "Email already exist" });
			}

			if (await _context.Users.Where(x => x.UserName == model.UserName).AnyAsync())
			{
				throw new RestException(HttpStatusCode.BadRequest, new { UserName = "UserName already exist" });
			}

			var user = new AppUser
			{
				DisplayName = model.DisplayName,
				Email = model.Email,
				UserName = model.UserName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				return Ok(new UserModel
				{
					DisplayName = user.DisplayName,
					Token = _jwtGenerator.CreateToken(user),
					UserName = user.UserName,
					Image = null
				});
			}

			throw new Exception("Client creation failed");
		}
    }
}
