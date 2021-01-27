using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;

namespace EFData
{
    public class DataSeed
    {
        public static async Task<IEnumerable<AppUser>> SeedDataAsync(DataContext context, UserManager<AppUser> userManager)
        {
            if (!EnumerableExtensions.Any(userManager.Users))
            {
                var users = new List<AppUser>
                                {
                                    new AppUser
                                        {
                                            DisplayName = "TestUserFirst",
                                            UserName = "TestUserFirst",
                                            Email = "testuserfirst@test.com"
                                        },

                                    new AppUser
                                        {
                                            DisplayName = "TestUserSecond",
                                            UserName = "TestUserSecond",
                                            Email = "testusersecond@test.com"
										}
                                };

				

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "qazwsX123@");
                }

				return users;
			}

			return new AppUser[0];
		}

		public static async Task SeedRolesAsync(DataContext context, RoleManager<AppRole> roleManager)
		{
			await roleManager.CreateAsync(new AppRole()
			{
				Name = "User",
				ConcurrencyStamp = new Random().Next(Int32.MaxValue).ToString(),
				NormalizedName = "user",
			});

			await roleManager.CreateAsync(new AppRole()
			{
				Name = "Admin",
				ConcurrencyStamp = new Random().Next(Int32.MaxValue).ToString(),
				NormalizedName = "admin",
			});
        }

		public static async Task SeedUserRoles(UserManager<AppUser> userManager, IEnumerable<AppUser> users)
		{
			foreach (var appUser in users)
			{
				await userManager.AddToRoleAsync(appUser, "Admin");
			}
		}
	}
}