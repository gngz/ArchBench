using System;
using HttpServer;
using System.Net;
namespace ArchBench.PlugIns.Broker.Utils
{
    public class CookieAdapter 
    {
        static public ResponseCookie adaptFromCookie(Cookie aCookie)
        {
            return new ResponseCookie(aCookie.Name, aCookie.Value, aCookie.Expires, aCookie.Path, aCookie.Domain);
        }

        static public Cookie adaptFromResponseCookie(ResponseCookie aCookie)
        {
            var cookie = new Cookie(aCookie.Name, aCookie.Value, aCookie.Path, null);
            cookie.Expires = aCookie.Expires;

            return cookie;
        }
    }
}
