using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;

namespace EFData
{
    public class DataSeed
    {
        public static async Task SeedDataAsync(DataContext context, UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
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
            }
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
	}
}