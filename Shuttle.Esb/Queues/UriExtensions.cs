﻿using System;

namespace Shuttle.Esb
{
    public static class UriExtensions
    {
        public static Uri Secured(this Uri uri)
        {
            return new UriBuilder(uri) {UserName = string.Empty, Password = string.Empty}.Uri;
        }
    }
}