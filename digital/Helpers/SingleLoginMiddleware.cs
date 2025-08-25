namespace digital.Helpers
{
    public class SingleLoginMiddleware
    {

        private readonly RequestDelegate _next;

        public SingleLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true ||
                !string.IsNullOrEmpty(context.Session.GetString("UserEmail")))
            {
                var email = context.Session.GetString("UserEmail");
                var sessionId = context.Session.GetString("SessionId");

                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.CurrentSessionId) || user.CurrentSessionId != sessionId)
                    {
                        context.Session.Clear();
                        context.Response.Redirect("/Home/Login");
                        return;
                    }
                }

            }

            await _next(context);
        }
    }
}

