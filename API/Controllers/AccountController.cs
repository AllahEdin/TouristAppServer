using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using Application.Exceptions;
using Domain;
using EFData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Services.Interfaces;

namespace API.Controllers
{
	
	[Route("")]
	public class AccountController : BaseController
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly DataContext _context;
		private readonly IJwtGenerator _jwtGenerator;

		public AccountController(UserManager<AppUser> userManager,
			DataContext context, 
			IJwtGenerator jwtGenerator)
		{
			_userManager = userManager;
			_context = context;
			_jwtGenerator = jwtGenerator;
		}

		[HttpGet("LoginWithGoogle")]
		[AllowAnonymous]
		public async Task<IActionResult> Login()
		{
			return Ok($"https://accounts.google.com/o/oauth2/v2/auth?" +
					$"client_id=834829378246-po41ui5mdf7hn1dqt58785f81r25rqh7.apps.googleusercontent.com" +
					$"&access_type=offline" +
					$"&redirect_uri=https://localhost:44397/googlesingin" +
					$"&response_type=code" +
					$"&scope=https://www.googleapis.com/auth/userinfo.profile");
		}

		//Вот эту херь зря сделал
		[HttpGet("googlesingin")]
		[AllowAnonymous]
		public async Task<IActionResult> Onlogon(string code, string scope)
		{
			HttpClient client = 
				new HttpClient();
			
			HttpResponseMessage res = await 
				client.PostAsync("https://oauth2.googleapis.com/token?" +
										"client_id=834829378246-po41ui5mdf7hn1dqt58785f81r25rqh7.apps.googleusercontent.com" +
										$"&client_secret=Z78d-njL0A6z2rULVgEaXFuj" + //TODO
										$"&code={code}" +
										$"&grant_type=authorization_code" +
										$"&redirect_uri=https://localhost:44397/googlesingin",
				null);

			if (res.IsSuccessStatusCode)
			{
				var googleResultStr =
					await res.Content.ReadAsStringAsync();

				GoogleSuccessResultModel model =
					JsonConvert.DeserializeObject<GoogleSuccessResultModel>(googleResultStr);

				HttpContent c =
					 new StringContent(googleResultStr, Encoding.UTF8, "application/json");

				var xxx =await 
					client.PostAsync("https://localhost:44397/TokenForGoogleUser", c);
			}

			return Ok();
		}

		[AllowAnonymous]
		[HttpPost(nameof(TokenForGoogleUser))]
		public async Task<IActionResult> TokenForGoogleUser([FromBody] GoogleSuccessResultModel model)
		{
			HttpClient client =
				new HttpClient();

			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {model.access_token}");

			var metaResult =
				await client.GetAsync("https://people.googleapis.com/v1/people/me" +
									"?personFields=names");

			if (metaResult.IsSuccessStatusCode)
			{
				var metaContent =
					await metaResult.Content.ReadAsStringAsync();

				var metaInfo =
					JsonConvert.DeserializeObject<GoogleMetaInfo>(metaContent);

				var meta =
					metaInfo.names.Single();

				var user = await _userManager.FindByEmailAsync(meta.metadata.source.id);

				UserModel usr = null;

				if (user == null)
				{
					usr =
						await RegisterNewUserAsync(meta.metadata.source.id, meta.displayName);
				}
				else
				{
					usr =
						new UserModel()
						{
							DisplayName = user.DisplayName,
							Image = null,
							Token = _jwtGenerator.CreateToken(user),
							UserName = user.UserName,
						};
				}

				return Ok(usr);
			}

			throw new InvalidOperationException();
		}

		private async Task<UserModel> RegisterNewUserAsync(string emailGoogleId, string userName)
		{
			userName =
				userName.Replace(" ", "");

			if (await _context.Users.Where(x => x.Email == emailGoogleId).AnyAsync())
			{
				throw new RestException(HttpStatusCode.BadRequest, new { Email = "Email already exist" });
			}

			if (await _context.Users.Where(x => x.UserName == userName).AnyAsync())
			{
				throw new RestException(HttpStatusCode.BadRequest, new { UserName = "UserName already exist" });
			}

			var user = new AppUser
			{
				DisplayName = emailGoogleId,
				Email = emailGoogleId,
				UserName = emailGoogleId
			};

			var result = await _userManager.CreateAsync(user, GeneratePassword(20));

			if (result.Succeeded)
			{
				return new UserModel()
				{
					DisplayName = user.DisplayName,
					Token = _jwtGenerator.CreateToken(user),
					UserName = user.UserName,
					Image = null
				};
			}

			throw new InvalidOperationException("Registration failed!!");
		}

		private string GeneratePassword(int length)
		{
			string s = "";

			bool hasInt = false;
			bool hasLow = false;
			bool hasUp = false;
			bool hasNon = false;
			int all = 0;

			var r = new Random();

			for (int i = 0; i < length; i++)
			{
				var n =
					r.Next(4);

				if (length - i <= (4 - all))
				{
					n = !hasInt
						? 0
						: !hasLow
							? 1
							: !hasUp
								? 2
								: 3;
				}

				switch (n)
				{
					case 0:
						if (!hasInt)
							all++;
						hasInt = true;
						s += r.Next(10).ToString();
						break;
					case 1:
						if (!hasLow)
							all++;
						hasLow = true;
						int num = r.Next(0, 26); // Zero to 25
						char let = (char)('a' + num);
						s += let;
						break;
					case 2:
						if (!hasUp)
							all++;
						hasUp = true;
						int num2 = r.Next(0, 26); // Zero to 25
						char let2 = (char)('A' + num2);
						s += let2;
						break;
					case 3:
						if (!hasNon)
							all++;
						hasNon = true;
						s += '!';
						break;
				}
			}

			return s;
		}

	}
}