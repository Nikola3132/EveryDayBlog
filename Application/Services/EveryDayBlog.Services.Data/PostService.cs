﻿namespace EveryDayBlog.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EveryDayBlog.Data.Common.Repositories;
    using EveryDayBlog.Data.Models;
    using EveryDayBlog.Data.Models.Enums;
    using EveryDayBlog.Services.Mapping;
    using EveryDayBlog.Web.ViewModels.PageHeaders.InputModels;
    using EveryDayBlog.Web.ViewModels.Posts.InputModels;
    using EveryDayBlog.Web.ViewModels.Posts.ViewModels;
    using EveryDayBlog.Web.ViewModels.Sections.InputModels;
    using Microsoft.EntityFrameworkCore;
    using X.PagedList;

    public class PostService : IPostService
    {
        private readonly IDeletableEntityRepository<Post> posts;
        private readonly IUsersService usersService;
        private readonly IPageHeaderService pageHeaderService;
        private readonly ISectionService sectionService;

        public PostService(
            IDeletableEntityRepository<Post> posts,
            IUsersService usersService,
            IPageHeaderService pageHeaderService,
            ISectionService sectionService)
        {
            this.posts = posts;
            this.usersService = usersService;
            this.pageHeaderService = pageHeaderService;
            this.sectionService = sectionService;
        }

        public async Task<bool> AddSectionToPostAsync(int postId, SectionInputModel sectionInputModel)
        {
            var currentPost = await this.posts.All()
                .SingleOrDefaultAsync(p => p.Id == postId);

            var sectionForAdding = await this.sectionService.CreateSectionServiceOnlyAsync(sectionInputModel);

            currentPost.PostSections.Add(new SectionPost { Section = sectionForAdding });

            int savedChanges = await this.posts.SaveChangesAsync();

            return savedChanges > 0;
        }

        public async Task<bool> CreatePostAsync(PostInputModel postInputModel, string username)
        {
            var currentUser = await this.usersService.GetUserByUsernameAsync(username);

            int pageHeaderId = await this.pageHeaderService.CreatePageHeaderAsync(postInputModel.PageHeader);

            var section = await this.sectionService.CreateSectionServiceOnlyAsync(postInputModel.Section);

            var postForDb = new Post
            {
                CreatedOn = DateTime.UtcNow,
                User = currentUser,
                UserId = currentUser.Id,
                PageHeaderId = pageHeaderId,
            };

            postForDb.PostSections.Add(new SectionPost { Section = section });

            await this.posts.AddAsync(postForDb);

            return await this.posts.SaveChangesAsync() > 0;
        }

        public async Task<TEntity> GetPostByIdAsync<TEntity>(int postId)
        {
            return await this.posts.All()
                .Where(p => p.Id == postId)
                .To<TEntity>()
                .SingleOrDefaultAsync();
        }

        public IEnumerable<TEntity> GetPostsBySearch<TEntity>(string searchString)
        {
            var searchStringClean = searchString
                .Split(
                    new string[]
                {
                    ",", ".", " ",
                }, StringSplitOptions.RemoveEmptyEntries);

            IQueryable<Post> posts = this.posts
                .All()
                .Include(p => p.PageHeader)
                .Where(x => searchStringClean
                .All(c => x.PageHeader.Title.ToLower().Contains(c.ToLower())));

            return posts.To<TEntity>();
        }

        public IEnumerable<TEntity> GetPostsFilter<TEntity>(string searchString)
        {
            if (searchString != null)
            {
                return this.GetPostsBySearch<TEntity>(searchString);
            }

            return this.GetVisiblePosts<TEntity>();
        }

        public IEnumerable<TEntity> GetVisiblePosts<TEntity>()
        {
            var notDeletedPosts = this.posts.All()
                             .Include(x => x.User)
                             .ThenInclude(u => u.Country)
                             .Include(p => p.PageHeader)
                             .ThenInclude(x => x.Image);

            return notDeletedPosts
                             .To<TEntity>();
        }

        public async Task<IEnumerable<IndexPostViewModel>> OrderByAsync(IEnumerable<IndexPostViewModel> posts, PostsSort sortBy, string username = null)
        {
            if (sortBy == PostsSort.Oldest)
            {
                return await posts.OrderBy(p => p.CreatedOn).ToListAsync();
            }
            else if (sortBy == PostsSort.Yours)
            {
                return await posts.Where(p => p.UserEmail == username).ToListAsync();
            }

            // Products SortType.Newest
            return await posts.OrderByDescending(p => p.CreatedOn).ToListAsync();
        }

        public async Task<bool> HidePostByIdAsync(int postId)
        {
            var currentPost = this.posts.All().FirstOrDefault(p => p.Id == postId);
            if (currentPost == null)
            {
                return false;
            }

            this.posts.Delete(currentPost);

            return await this.posts.SaveChangesAsync() > 0;
        }

        public async Task<int> GetPageHeaderIdAsync(int postId)
        {
            var post = await this.posts.All().Include(p => p.PageHeader).SingleOrDefaultAsync(p1 => p1.Id == postId);

            return post.PageHeader.Id;
        }

        public async Task<string> GetCreatorsIdAsync(int postId)
        {
            var post = await this.posts.All().SingleOrDefaultAsync(p => p.Id == postId);

            return post.UserId;
        }

        public async Task<List<TEntity>> AllHidenPosts<TEntity>()
        {
            return await this.posts.AllWithDeleted().Where(p => p.IsDeleted == true).To<TEntity>().ToListAsync();
        }

        public async Task<bool> MakeVisibleAsync(int postId)
        {
            var deletedCurrentPost = await this.posts.AllWithDeleted().SingleOrDefaultAsync(p => p.Id == postId && p.IsDeleted);

            if (deletedCurrentPost == null)
            {
                return false;
            }

            deletedCurrentPost.IsDeleted = false;

            this.posts.Update(deletedCurrentPost);

            return await this.posts.SaveChangesAsync() > 0;
        }
    }
}
