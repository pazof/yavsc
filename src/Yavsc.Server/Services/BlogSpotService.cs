#nullable enable annotations

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Server.Exceptions;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;

public class BlogSpotService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileSystemAuthManager fileSystemAuthManager;
    private readonly SiteSettings siteSettings;
    private readonly ILogger<BlogSpotService> logger;

    public BlogSpotService(
        ApplicationDbContext context,
        IAuthorizationService authorizationService,
        IFileSystemAuthManager fileSystemAuthManager,
        IOptions<SiteSettings> siteSettings,
        ILoggerFactory loggerFactory)
    {
        _authorizationService = authorizationService;
        _context = context;
        this.fileSystemAuthManager = fileSystemAuthManager;
        this.siteSettings = siteSettings.Value;
        if (siteSettings.Value.Blog == null)
        {
            throw new InvalidOperationException("SiteSettings.Blog is not configured.");
        }
        this.logger = loggerFactory.CreateLogger<BlogSpotService>();
    }

    public void AttachFiles(IFormFileCollection files, string userId, long postId)
    {
        // Traiter les fichiers attaches s'il y en a
        if (files != null && files.Count > 0)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new InvalidOperationException($"Utilisateur {userId} non trouvé.");

            try
            {
                string blogFilesSubdir = $"blogs/{postId}";
                string destDir = Path.Combine(
                    siteSettings.Blog,
                    user.UserName,
                    blogFilesSubdir);
                var di = new DirectoryInfo(destDir);

                if (!di.Exists)
                {
                    logger.LogInformation("Creating directory for blog attachments: {destDir}", destDir);
                    di.Create();
                }

                foreach (var formFile in files)
                {
                    var fileInfo = user.ReceiveUserFile(destDir, formFile);
                    if (fileInfo != null && !fileInfo.QuotaOffense)
                    {
                        logger.LogInformation("Attached file {fileName} to blog post {postId} for user {userId}.", fileInfo.FileName, postId, userId);
                        var uploadedFile = new UploadedFile
                        {
                            Path = fileInfo.FileName,
                            ContentType = formFile.ContentType,
                            Length = formFile.Length
                        };
                        _context.UploadedFiles.Add(uploadedFile);
                        _context.SaveChanges(userId);

                        var attachment = new BlogAttachedFile
                        {
                            PostId = postId,
                            FileId = uploadedFile.Id
                        };
                        _context.BlogAttachedFiles.Add(attachment);
                        logger.LogInformation("Created BlogAttachedFile entry for file {fileName} and blog post {postId}.", fileInfo.FileName, postId);
                    }
                    else
                    {
                        logger.LogWarning("Failed to attach file {fileName} to blog post {postId} for user {userId}. Quota offense: {quotaOffense}", formFile.FileName, postId, userId, fileInfo?.QuotaOffense);
                    }
                }
                _context.SaveChanges(userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du traitement des fichiers : {ex.Message}");
                logger.LogError(ex, "Error while processing attached files for blog post {postId} and user {userId}.", postId, userId);
            }
        }
    }

    public BlogPost Create(string userId, BlogPost post )
    {
        // Sauvegarder le post d'abord pour obtenir son ID
        // Le créateur vient de l'authentification, donc on ne le prend pas du post
        post.AuthorId = userId;
        _context.BlogSpot.Add(post);
        _context.SaveChanges(userId);
        return post;
    }

    public async Task<BlogPostEditViewModel> GetPostForEdition(ClaimsPrincipal user, long blogPostId)
    {
        var blog = await _context.BlogSpot
            .Include(x => x.Author)
            .Include(x => x.ACL)
            .SingleAsync(m => m.Id == blogPostId);

        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
            throw new AuthorizationFailureException(auth);

        var pub = await _context.blogSpotPublications.AnyAsync(x => x.PostId == blog.Id);
        ScrubAclForViewer(blog, user);

        return new BlogPostEditViewModel(blog, pub);
    }

    public async Task<BlogPost> Details(ClaimsPrincipal user, long blogPostId)
    {
        BlogPost blog = await _context.BlogSpot
            .Include(p => p.Author)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .Include(p => p.ACL)
            .Include(p => p.Publication)
            .SingleAsync(m => m.Id == blogPostId);

        if (blog == null)
            return null;

        var auth = await _authorizationService.AuthorizeAsync(user, blog, new ReadPermission());
        if (!auth.Succeeded)
            throw new AuthorizationFailureException(auth);

        ScrubAclForViewer(blog, user);

        foreach (var c in blog.Comments)
            c.Author = _context.Users.First(u => u.Id == c.AuthorId);

        return blog;
    }

    public async Task Modify(ClaimsPrincipal user, BlogPostEditViewModel blogEdit)
    {
        var blog = _context.BlogSpot.SingleOrDefault(b => b.Id == blogEdit.Id);
        Debug.Assert(blog != null);

        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
            throw new AuthorizationFailureException(auth);

        blog.Article = blogEdit.Article;
        blog.Title = blogEdit.Title;
        blog.Photo = blogEdit.Photo;
        blog.ACL = blogEdit.ACL;
        _context.Update(blog);

        var publication = await _context.blogSpotPublications
            .SingleOrDefaultAsync(p => p.PostId == blogEdit.Id);

        if (publication != null)
        {
            if (!blogEdit.Publish)
                _context.blogSpotPublications.Remove(publication);
        }
        else if (blogEdit.Publish)
        {
            _context.blogSpotPublications.Add(new BlogSpotPublication { PostId = blogEdit.Id });
        }

        _context.SaveChanges(user.GetUserId());
    }

    public async Task Modify(ClaimsPrincipal user, BlogPost blog)
    {
        var existing = await _context.BlogSpot
            .Include(b => b.ACL)
            .SingleOrDefaultAsync(b => b.Id == blog.Id);

        if (existing == null)
            throw new InvalidOperationException($"Blog post {blog.Id} not found.");

        var auth = await _authorizationService.AuthorizeAsync(user, existing, new EditPermission());
        if (!auth.Succeeded)
            throw new AuthorizationFailureException(auth);

        existing.Title = blog.Title;
        existing.Article = blog.Article;
        existing.Photo = blog.Photo;
        existing.ACL = blog.ACL;

        _context.Update(existing);
        _context.SaveChanges(user.GetUserId());
    }


    public async Task<IEnumerable<IBlogPost>> Index(ClaimsPrincipal user, string id, int skip = 0, int take = 25)
    {
        IEnumerable<IBlogPost> posts;

        if (user.Identity.IsAuthenticated)
        {
            string viewerId = user.GetUserId();
            long[] userCircles = await _context.Circle.Include(c => c.Members)
                .Where(c => c.Members.Any(m => m.MemberId == viewerId))
                .Select(c => c.Id)
                .ToArrayAsync();

            posts = _context.BlogSpot
                .Include(b => b.Author)
                .Include(p => p.ACL)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Include(p => p.Publication)
                .Where(p => p.Publication != null
                    || p.AuthorId == viewerId
                    || (userCircles != null && p.ACL.Any(a => userCircles.Contains(a.CircleId))));
        }
        else
        {
            posts = _context.blogSpotPublications
                .Include(p => p.Post)
                .Include(b => b.Post.Author)
                .Include(p => p.Post.ACL)
                .Include(p => p.Post.Tags)
                .Include(p => p.Post.Comments)
                .Select(p => p.Post)
                .ToArray();
        }

        var materialized = posts.ToList();

        foreach (var post in materialized.OfType<BlogPost>())
            ScrubAclForViewer(post, user);

        return materialized
            .OrderByDescending(p => p.DateModified)
            .Skip(skip)
            .Take(take);
    }

    public async Task Delete(ClaimsPrincipal user, long id)
    {
        BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);
        _context.BlogSpot.Remove(blog);
        _context.SaveChanges(user.GetUserId());
    }

    public async Task<IEnumerable<BlogPost>> UserPosts(string posterName, string? readerId, int pageLen = 10, int pageNum = 0)
    {
        string? posterId = (await _context.Users.SingleOrDefaultAsync(u => u.UserName == posterName))?.Id;
        if (posterId == null) return Array.Empty<BlogPost>();

        var posts = _context.UserPosts(posterId, readerId).ToList();
        var isOwnerReader = string.Equals(readerId, posterId, StringComparison.Ordinal);

        foreach (var post in posts)
        {
            if (!isOwnerReader)
                post.ACL = new List<CircleAuthorizationToBlogPost>();
        }

        return posts;
    }

    public object? GetTitle(string title)
    {
        return _context.BlogSpot
            .Include(b => b.Author)
            .Where(x => x.Title == title)
            .OrderByDescending(x => x.DateCreated)
            .ToList();
    }

    public async Task<BlogPost?> GetBlogPostAsync(long value)
    {
        return await _context.BlogSpot
            .Include(b => b.Author)
            .Include(b => b.ACL)
            .SingleOrDefaultAsync(x => x.Id == value);
    }

    public async Task<bool> SetPublishAsync(ClaimsPrincipal user, long postId, bool publish)
    {
        var blog = await _context.BlogSpot.SingleOrDefaultAsync(b => b.Id == postId);
        if (blog == null) return false;

        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
            throw new AuthorizationFailureException(auth);

        var existing = await _context.blogSpotPublications.SingleOrDefaultAsync(p => p.PostId == postId);
        if (publish)
        {
            if (existing == null)
                _context.blogSpotPublications.Add(new BlogSpotPublication { PostId = postId });
        }
        else
        {
            if (existing != null)
                _context.blogSpotPublications.Remove(existing);
        }

        await _context.SaveChangesAsync(user.GetUserId());
        return true;
    }

    private static void ScrubAclForViewer(BlogPost post, ClaimsPrincipal? user)
    {
        if (!IsOwner(post, user))
            post.ACL = new List<CircleAuthorizationToBlogPost>();
    }

    private static bool IsOwner(BlogPost post, ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        return string.Equals(user.GetUserId(), post.AuthorId, StringComparison.Ordinal);
    }
}
