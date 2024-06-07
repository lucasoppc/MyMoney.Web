


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyMoney.Web.Services;

namespace MyMoney.Web.Filters;

public class AuthenticationFilter(CacheService cache) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var token = await cache.GetValueAsync<string>("UserToken");
        var isAuthenticated = !string.IsNullOrEmpty(token);

        if (!isAuthenticated)
        {
            context.Result = new RedirectToPageResult("/login");
            return;
        }

        await next();
    }
}