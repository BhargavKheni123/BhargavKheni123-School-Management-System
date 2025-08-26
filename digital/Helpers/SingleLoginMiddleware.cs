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
            var email = context.Session.GetString("UserEmail");
            var sessionId = context.Session.GetString("SessionId");

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(sessionId))
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    context.Session.Clear();
                }
                else if (!user.IsLoggedIn)
                {
                    context.Session.Clear();
                }
                else if (!string.IsNullOrEmpty(user.CurrentSessionId) &&
                         user.CurrentSessionId != sessionId)
                {
                    context.Session.Clear();
                    context.Response.Redirect("/Home/Login");
                    return;
                }
            }

            await _next(context);
        }

    }
}
