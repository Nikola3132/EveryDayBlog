﻿namespace EveryDayBlog.Web.Areas.Identity.Pages.Account.Manage
{
    using System.Threading.Tasks;

    using EveryDayBlog.Data.Models;
    using EveryDayBlog.Web.ViewModels.PageHeaders.ViewModels;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

#pragma warning disable SA1649 // File name should match first type name
    public class PersonalDataModel : PageModel
#pragma warning restore SA1649 // File name should match first type name
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<PersonalDataModel> logger;

        public PageHeaderViewModel PageHeader { get; set; }

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound(value: $"Unable to load user with ID '{this.userManager.GetUserId(this.User)}'.");
            }

            return this.Page();
        }
    }
}
