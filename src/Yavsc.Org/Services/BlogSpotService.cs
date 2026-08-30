using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Server.Exceptions;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;


[Obsolete]
public class OldBlogSpotService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IFileSystemAuthManager fileSystemAuthManager;

    public OldBlogSpotService(ApplicationDbContext context,
    IAuthorizationService authorizationService,
    IFileSystemAuthManager fileSystemAuthManager)
    {
        _authorizationService = authorizationService;
        _context = context;
        this.fileSystemAuthManager = fileSystemAuthManager;
    }

    public Yavsc.Models.Blog.BlogPost Create(string userId, Yavsc.Models.Blog.BlogPost post, IFormFileCollection files)
    {
        // Sauvegarder le post d'abord pour obtenir son ID
        _context.BlogSpot.Add(post);
        _context.SaveChanges(userId);

        // Traiter les fichiers attachés s'il y en a
        if (files != null && files.Count > 0)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                try
                {
                    // Créer un répertoire pour les fichiers du blog
                    string blogFilesSubdir = $"blogs/{post.Id}";
                    string destDir = Path.Combine(
                        AbstractFileSystemHelpers.UserFilesDirName,
                        user.UserName,
                        blogFilesSubdir
                    );
                    var di = new DirectoryInfo(destDir);
                    if (!di.Exists) di.Create();

                    // Traiter chaque fichier
                    foreach (var formFile in files)
                    {
                        var fileInfo = user.ReceiveUserFile(destDir, formFile);
                        if (fileInfo != null && !fileInfo.QuotaOffense)
                        {
                            // Créer une entrée UploadedFile si nécessaire
                            var uploadedFile = new UploadedFile
                            {
                                Path = fileInfo.FileName,
                                ContentType = formFile.ContentType,
                                Length = formFile.Length
                            };
                            _context.UploadedFiles.Add(uploadedFile);
                            _context.SaveChanges(userId);

                            // Lier le fichier au post
                            var attachment = new BlogAttachedFile
                            {
                                PostId = post.Id,
                                FileId = uploadedFile.Id
                            };
                            _context.BlogAttachedFiles.Add(attachment);
                        }
                    }
                    _context.SaveChanges(userId);
                }
                catch (Exception ex)
                {
                    // Logger l'erreur mais ne pas échouer la création du post
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du traitement des fichiers : {ex.Message}");
                }
            }
        }

        return post;
    }
    public async Task<BlogPostEditViewModel> GetPostForEdition(ClaimsPrincipal user, long blogPostId)
    {
        var blog = await _context.BlogSpot.Include(x => x.Author).Include(x => x.ACL).SingleAsync(m => m.Id == blogPostId);
        var auth = await _authorizationService.AuthorizeAsync(user, blog, new EditPermission());
        if (!auth.Succeeded)
        {
            throw new AuthorizationFailureException(auth);
        }
        var pub = await _context.blogSpotPublications.AnyAsync(x => x.BlogpostId == blog.Id);
        ScrubAclForViewer(blog, user);

        return new BlogPostEditViewModel(blog, pub);
    }

    public async Task<Yavsc.Models.Blog.BlogPost> Details(ClaimsPrincipal user, long blogPostId)
    {
        Yavsc.Models.Blog.BlogPost blog = await _context.BlogSpot
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
        ScrubAclForViewer(blog, user);
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
        blog.Article = blogEdit.Article;
        blog.Title = blogEdit.Title;
        blog.Photo = blogEdit.Photo;
        blog.ACL = blogEdit.ACL;
        // saves the change
        _context.Update(blog);
        var publication = await _context.blogSpotPublications.SingleOrDefaultAsync
        (p => p.BlogpostId == blogEdit.Id);
        if (publication != null)
        {
            if (!blogEdit.Publish)
            {
                _context.blogSpotPublications.Remove(publication);
            }
        }
        else
        {
            if (blogEdit.Publish)
            {
                _context.blogSpotPublications.Add(
                    new BlogSpotPublication
                    {
                        BlogpostId = blogEdit.Id
                    }
                );
            }
        }
        _context.SaveChanges(user.GetUserId());
    }

    public async Task Modify(ClaimsPrincipal user, Yavsc.Models.Blog.BlogPost blog)
    {
        var existing = await _context.BlogSpot.Include(b => b.ACL).SingleOrDefaultAsync(b => b.Id == blog.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Blog post {blog.Id} not found.");
        }

        var auth = await _authorizationService.AuthorizeAsync(user, existing, new EditPermission());
        if (!auth.Succeeded)
        {
            throw new AuthorizationFailureException(auth);
        }

        existing.Title = blog.Title;
        existing.Article = blog.Article;
        existing.Photo = blog.Photo;
        existing.ACL = blog.ACL;

        _context.Update(existing);
        _context.SaveChanges(user.GetUserId());
    }

    public async Task<IEnumerable<IBlogPost>> Index(ClaimsPrincipal user, string id, int skip = 0, int take = 25)
    {
        string? viewerId = user.Identity?.IsAuthenticated == true ? user.GetUserId() : null;
        IEnumerable<IBlogPost> posts;

        if (user.Identity.IsAuthenticated)
        {
            string viewerIdNonNull = viewerId!;
            long[] userCircles = await _context.Circle.Include(c => c.Members).
                Where(c => c.Members.Any(m => m.MemberId == viewerIdNonNull))
                .Select(c => c.Id).ToArrayAsync();

            posts = _context.BlogSpot
                .Include(b => b.Author)
                .Include(p => p.ACL)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.ACL == null
                || p.ACL.Count == 0
                || (p.AuthorId == viewerIdNonNull)
                || (userCircles != null &&
                    p.ACL.Any(a => userCircles.Contains(a.CircleId)))
                );
        }
        else
        {
            posts = _context.blogSpotPublications
            .Include(p => p.BlogPost)
           .Include(b => b.BlogPost.Author)
           .Include(p => p.BlogPost.ACL)
           .Include(p => p.BlogPost.Tags)
           .Include(p => p.BlogPost.Comments)
           .Where(p => p.BlogPost.ACL == null
           || p.BlogPost.ACL.Count == 0)
           .Select(p => p.BlogPost).ToArray();
        }

        var materialised = posts.ToList();
        foreach (var post in materialised.OfType<Yavsc.Models.Blog.BlogPost>())
            ScrubAclForViewer(post, user);

        var data = materialised.OrderByDescending(p => p.DateModified)
            .Skip(skip)
            .Take(take);
        return data;
    }

    public async Task Delete(ClaimsPrincipal user, long id)
    {
        var uid = user.GetUserId();
        Yavsc.Models.Blog.BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);

        _context.BlogSpot.Remove(blog);
        _context.SaveChanges(user.GetUserId());
    }

    public async Task<IEnumerable<Yavsc.Models.Blog.BlogPost>> UserPosts(
        string posterName,
        string? readerId,
        int pageLen = 10,
        int pageNum = 0)
    {
        string? posterId = (await _context.Users.SingleOrDefaultAsync(u => u.UserName == posterName))?.Id ?? null;
        if (posterId == null) return Array.Empty<Yavsc.Models.Blog.BlogPost>();
        var posts = _context.UserPosts(posterId, readerId).ToList();
        var viewerId = string.Equals(readerId, posterId, StringComparison.Ordinal) ? readerId : null;
        foreach (var post in posts)
            ScrubAclForViewer(post, viewerId);
        return posts;
    }

    public object? GetTitle(string title)
    {
        return _context.BlogSpot.Include(
                 b => b.Author
             ).Where(x => x.Title == title).OrderByDescending(
                 x => x.DateCreated
             ).ToList();
    }

    public async Task<Yavsc.Models.Blog.BlogPost?> GetBlogPostAsync(long value)
    {
        return await _context.BlogSpot
        .Include(b => b.Author)
        .Include(b => b.ACL)
        .SingleOrDefaultAsync(x => x.Id == value);
    }

    private static void ScrubAclForViewer(Yavsc.Models.Blog.BlogPost post, ClaimsPrincipal? user)
    {
        if (!IsOwner(post, user))
            post.ACL = new List<CircleAuthorizationToBlogPost>();
    }

    private static void ScrubAclForViewer(Yavsc.Models.Blog.BlogPost post, string? viewerId)
    {
        if (!string.Equals(post.AuthorId, viewerId, StringComparison.Ordinal)
            && !string.Equals(post.Author?.Id, viewerId, StringComparison.Ordinal))
            post.ACL = new List<CircleAuthorizationToBlogPost>();
    }

    private static bool IsOwner(Yavsc.Models.Blog.BlogPost post, ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;

        var viewerId = user.GetUserId();
        var viewerName = user.GetUserName() ?? user.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(viewerId))
        {
            if (string.Equals(post.AuthorId, viewerId, StringComparison.Ordinal)) return true;
            if (string.Equals(post.Author?.Id, viewerId, StringComparison.Ordinal)) return true;
        }

        if (!string.IsNullOrWhiteSpace(viewerName))
        {
            if (string.Equals(post.AuthorId, viewerName, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(post.Author?.UserName, viewerName, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

}
