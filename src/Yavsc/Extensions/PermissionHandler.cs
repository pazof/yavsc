using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RazorEngine.Compilation.ImpromptuInterface.Optimization;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.ViewModels.Auth;

namespace Yavsc.Extensions;

public class PermissionHandler : IAuthorizationHandler
{
    ApplicationDbContext applicationDbContext;
    public PermissionHandler(ApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is ReadPermission)
            {
                if (IsPublic(context.Resource))
                {
                    context.Succeed(requirement);
                }
                else if (IsOwner(context.User, context.Resource)
                    || IsSponsor(context.User, context.Resource))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement is EditPermission || requirement is DeletePermission)
            {
                if (IsOwner(context.User, context.Resource))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }

    private bool IsPublic(object? resource)
    {
        if (resource is BlogPost blogPost)
        {
            if (blogPost.ACL.Count==0)
                return true;
        }
        return false;
    }

    private  bool IsOwner(ClaimsPrincipal user, object? resource)
    {
        if (resource is BlogPost blogPost)
        {
            return blogPost.AuthorId == user.GetUserId();
        }
        else
        if (resource is DefaultHttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/blogpost/Delete", StringComparison.OrdinalIgnoreCase))
            {
                string postId = (string)httpContext.GetRouteValue("id");
                if (long.TryParse(postId, out long id))
                {
                BlogPost b = applicationDbContext.BlogSpot.FirstOrDefault(b => b.Id == id && b.AuthorId == user.GetUserId());
                return b != null;

                }
            }
        }
        return false;
    }

    private bool IsSponsor(ClaimsPrincipal user, object? resource)
    {
        if (resource is BlogPost blogPost)
        {
            return applicationDbContext.CircleMembers
            .Include(c => c.Circle)
            .Where(m=>m.MemberId==user.GetUserId() && m.Circle.OwnerId == blogPost.OwnerId)
            .Any();
        }
        return true;
    }
}
