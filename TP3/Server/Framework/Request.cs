﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IFT585_TP3.Server.Framework
{
    public class Request
    {
        private HttpListenerRequest listenerRequest;
        private NameValueCollection _params;

        public Request(HttpListenerRequest listenerRequest, List<Tuple<string, int>> paramsTokens)
        {
            this.listenerRequest = listenerRequest;

            _params = new NameValueCollection();
            foreach (var token in paramsTokens)
            {
                var tokenValue = new Regex("[a-zA-Z0-9-._~\\%]*").Matches(BaseUrl)[token.Item2].Value;
                Params.Add(token.Item1, tokenValue);
            }
        }

        public string BaseUrl
        {
            get { return listenerRequest.Url.AbsolutePath; }
        }

        public NameValueCollection Params
        {
            get { return _params; }
        }

        public NameValueCollection Query
        {
            get { return listenerRequest.QueryString; }
        }
    }
}
