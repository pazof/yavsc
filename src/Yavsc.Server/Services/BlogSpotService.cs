

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Server.Exceptions;
using Yavsc.ViewModels.Auth;
using Yavsc.ViewModels.Blog;

public class BlogSpotService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;


    public BlogSpotService(ApplicationDbContext context,
    IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _context = context;
    }

    public BlogPost Create(string userId, BlogPostInputViewModel blogInput)
    {
        BlogPost post = new BlogPost
        {
            Title = blogInput.Title,
            Content = blogInput.Content,
            Photo = blogInput.Photo,
            AuthorId = userId
        };
        _context.BlogSpot.Add(post);
        _context.SaveChanges(userId);
        return post;
    }
    public async Task<BlogPost> GetPostForEdition(ClaimsPrincipal user, long blogPostId)
    {
        var blog = await _context.BlogSpot.Include(x => x.Author).Include(x => x.ACL).SingleAsync(m => m.Id == blogPostId);
        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
        {
            throw new AuthorizationFailureException(auth);
        }
        return blog;
    }

    public async Task<BlogPost> Details(ClaimsPrincipal user, long blogPostId)
    {

        BlogPost blog = await _context.BlogSpot
       .Include(p => p.Author)
       .Include(p => p.Tags)
       .Include(p => p.Comments)
       .Include(p => p.ACL)
       .SingleAsync(m => m.Id == blogPostId);
        if (blog == null)
        {
            return null;
        }
        var auth = await _authorizationService.AuthorizeAsync(user, blog, new ReadPermission());
        if (!auth.Succeeded)
        {
            throw new AuthorizationFailureException(auth);
        }
        foreach (var c in blog.Comments)
        {
            c.Author = _context.Users.First(u => u.Id == c.AuthorId);
        }
        return blog;
    }

    public async Task Modify(ClaimsPrincipal user, BlogPostEditViewModel blogEdit)
    {
        var blog = _context.BlogSpot.SingleOrDefault(b => b.Id == blogEdit.Id);
        Debug.Assert(blog != null);
        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
        {
            throw new AuthorizationFailureException(auth);
        }
        blog.Content = blogEdit.Content;
        blog.Title = blogEdit.Title;
        blog.Photo = blogEdit.Photo;
        blog.ACL = blogEdit.ACL;
        // saves the change
        _context.Update(blog);
        _context.SaveChanges(user.GetUserId());
    }

    public async Task<IEnumerable<IGrouping<String, BlogPost>>> IndexByTitle(ClaimsPrincipal user, string id, int skip = 0, int take = 25)
    {
        IEnumerable<BlogPost> posts;

        if (user.Identity.IsAuthenticated)
        {
            string viewerId = user.GetUserId();
            long[] usercircles = await _context.Circle.Include(c => c.Members).
                Where(c => c.Members.Any(m => m.MemberId == viewerId))
                .Select(c => c.Id).ToArrayAsync();

            posts = _context.BlogSpot
                .Include(b => b.Author)
                .Include(p => p.ACL)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => (p.ACL.Count == 0)
                || (p.AuthorId == viewerId)
                || (usercircles != null && p.ACL.Any(a => usercircles.Contains(a.CircleId)))
                );
        }
        else
        {
            posts = _context.blogspotPublications
            .Include(p => p.BlogPost)
           .Include(b => b.BlogPost.Author)
           .Include(p => p.BlogPost.ACL)
           .Include(p => p.BlogPost.Tags)
           .Include(p => p.BlogPost.Comments)
           .Where(p => p.BlogPost.ACL.Count == 0).Select(p => p.BlogPost).ToArray();
        }

        var data = posts.OrderByDescending(p => p.DateCreated);
        var grouped = data.GroupBy(p => p.Title).Skip(skip).Take(take);
        return grouped;
    }

    public async Task Delete(ClaimsPrincipal user, long id)
    {
        var uid = user.GetUserId();
        BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);

        _context.BlogSpot.Remove(blog);
        _context.SaveChanges(user.GetUserId());
    }

    public async Task<IEnumerable<BlogPost>> UserPosts(
        string posterName,
        string? readerId,
        int pageLen = 10,
        int pageNum = 0)
    {
        string? posterId = (await _context.Users.SingleOrDefaultAsync(u => u.UserName == posterName))?.Id ?? null;
        if (posterId == null) return Array.Empty<BlogPost>();
        return _context.UserPosts(posterId, readerId);
    }

    public object? ByTitle(string title)
    {
        return _context.BlogSpot.Include(
                 b => b.Author
             ).Where(x => x.Title == title).OrderByDescending(
                 x => x.DateCreated
             ).ToList();
    }
}
