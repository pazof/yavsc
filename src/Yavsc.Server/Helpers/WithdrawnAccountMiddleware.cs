using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;

namespace Yavsc.Server.Helpers;

public sealed class WithdrawnAccountMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var id = context.User.GetUserId();
        if (!string.IsNullOrWhiteSpace(id) && await db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == id && u.WithdrawalRequestedAt != null, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 401, Title = "Compte ferme", Detail = "Ce compte fait l'objet d'un retrait et ne peut plus etre utilise."
            }, context.RequestAborted);
            return;
        }
        await next(context);
    }
}
