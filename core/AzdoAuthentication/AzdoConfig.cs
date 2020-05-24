﻿namespace Glue.AzdoAuthentication
{
    public class AzdoConfig
    {
        public string AuthUrl { get; set; } = "https://app.vssps.visualstudio.com/oauth2/authorize";
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string RedirectUri { get; set; }
        public string TokenUrl { get; set; } = "https://app.vssps.visualstudio.com/oauth2/token";
        //public string AppSecret { get; set; }
        public string ClientSecret { get; set; }
    }
}