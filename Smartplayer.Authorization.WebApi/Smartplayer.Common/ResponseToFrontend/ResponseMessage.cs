using System;
using System.Collections.Generic;
using System.Text;

namespace Smartplayer.Common.ResponseToFrontend
{
    public static class ResponseMessage
    {
        public static string UserNotExsits() => $"Incorrect email or password";
        public static string UserExsits() => $"User with this email exists";
        public static string ProblemWithRefreshToken(string problem) => $"Problem with refresh: {problem}";
    }
}
