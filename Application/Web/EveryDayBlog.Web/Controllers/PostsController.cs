﻿namespace EveryDayBlog.Web.Controllers
{
    using System.Threading.Tasks;

    using EveryDayBlog.Services;
    using EveryDayBlog.Services.Data;
    using EveryDayBlog.Web.ViewModels.Posts.InputModels;
    using EveryDayBlog.Web.ViewModels.Posts.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize]
    public class PostsController : BaseController
    {
        private const string CreatingPostErrorMsg = "Something went wrong! We'll look at the problem and soon if all is well, your post will be uploaded!";
        private const string CreatingPostErrorLogMsg = "Post cannot be saved in the database.";
        private readonly IPostService postService;
        private readonly ILogger<PostsController> logger;
        private readonly IProtectionService protectionService;

        public PostsController(
            IPostService postService,
            ILogger<PostsController> logger,
            IProtectionService protectionService)
        {
            this.postService = postService;
            this.logger = logger;
            this.protectionService = protectionService;
        }

        [HttpGet]
        public IActionResult Create() => this.View();

        [HttpPost]
        public async Task<IActionResult> Create(PostInputModel post)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            var isPostCreated = await this.postService.CreatePostAsync(post, this.User.Identity.Name);

            if (!isPostCreated)
            {
                this.TempData["alert"] = CreatingPostErrorMsg;
                this.logger.LogError(CreatingPostErrorLogMsg);
            }

            return this.Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            this.TempData["ProductId"] = id;

            var post = await this.postService.GetPostByIdAsync<IndexPostViewModel>(id);
            if (post == null)
            {
                return this.NotFound();
            }

            return this.View(post);
        }
    }
}
