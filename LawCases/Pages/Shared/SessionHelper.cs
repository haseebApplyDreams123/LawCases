using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LawCases.Helpers
{
    public static class SessionHelper
    {
        public static bool IsUserLoggedIn(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString("UserId"));
        }

        public static string? GetUserId(HttpContext context)
        {
            return context.Session.GetString("UserId");
        }

        public static string? GetUserEmail(HttpContext context)
        {
            return context.Session.GetString("UserEmail");
        }

        public static string? GetUserName(HttpContext context)
        {
            return context.Session.GetString("UserName");
        }

        public static string? GetUserRole(HttpContext context)
        {
            return context.Session.GetString("UserRole");
        }

        public static DateTime? GetLoginTime(HttpContext context)
        {
            var loginTimeString = context.Session.GetString("LoginTime");
            if (DateTime.TryParse(loginTimeString, out var loginTime))
            {
                return loginTime;
            }
            return null;
        }

        public static void ClearSession(HttpContext context)
        {
            context.Session.Clear();
        }
    }

    // Extension methods for PageModel
    public static class PageModelExtensions
    {
        public static bool IsUserLoggedIn(this PageModel pageModel)
        {
            return SessionHelper.IsUserLoggedIn(pageModel.HttpContext);
        }

        public static string? GetCurrentUserId(this PageModel pageModel)
        {
            return SessionHelper.GetUserId(pageModel.HttpContext);
        }

        public static string? GetCurrentUserEmail(this PageModel pageModel)
        {
            return SessionHelper.GetUserEmail(pageModel.HttpContext);
        }

        public static string? GetCurrentUserName(this PageModel pageModel)
        {
            return SessionHelper.GetUserName(pageModel.HttpContext);
        }

        public static string? GetCurrentUserRole(this PageModel pageModel)
        {
            return SessionHelper.GetUserRole(pageModel.HttpContext);
        }
    }
}