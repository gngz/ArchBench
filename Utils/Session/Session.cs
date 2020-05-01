using HttpServer;
using System;
using System.Collections.Generic;

namespace ArchBench.PlugIns.Utils.Session
{
    public class Session
    {
        public string CookieName { get; set; }

        public DateTime Expiration { get; set; }

        public IDictionary<string, object> Vars { get; set; }

        private IDictionary<Guid, IDictionary<string, object>> _sessions;

        private Guid _session;

        public string Id { get => _session.ToString(); }

        private IHttpResponse _response;

        public Session(string cookieName)
        {
            _sessions = new Dictionary<Guid, IDictionary<string, object>>();
            CookieName = cookieName;
        }

        public void HandleSession(IHttpRequest aRequest, IHttpResponse aResponse)
        {
            var cookie = GetResponseCookie(aRequest);

            _response = aResponse;

            if (cookie != null)
            {
                Guid guid = new Guid(cookie.Value);
                if (_sessions.ContainsKey(guid))
                {
                    Vars = _sessions[guid];
                    _session = guid;
                }
                else
                {
                    Vars = null;
                }

            }
            else
            {
                Vars = null;
            }

        }

        public void StartSession(IHttpResponse aResponse)
        {
            Guid guid = Guid.NewGuid();
            _sessions.Add(guid, new Dictionary<string, object>());
            SetResponseCookie(aResponse, guid, Expiration);
            Vars = _sessions[guid];
            _session = guid;
        }

        public void EndSession(IHttpRequest aRequest, IHttpResponse aResponse)
        {
            var cookie = GetResponseCookie(aRequest);

            if (cookie != null)
            {
                Guid guid = new Guid(cookie.Value);
                if (_sessions.ContainsKey(guid))
                {
                    _sessions[guid].Clear();
                    _sessions.Remove(guid);
                    SetResponseCookie(aResponse, guid, DateTime.Now.AddHours(-1));
                    Vars = null;

                }
            }



        }

        public IDictionary<string, object> GetSessionVars()
        {
            return _sessions[_session];
        }

        private void SetResponseCookie(IHttpResponse aResponse, Guid guid, DateTime aExpiration)
        {
            aResponse.Cookies.Add(new RequestCookie(CookieName, guid.ToString()), aExpiration);

        }

        private RequestCookie GetResponseCookie(IHttpRequest aRequest)
        {
            return aRequest.Cookies[CookieName];
        }

    }
}