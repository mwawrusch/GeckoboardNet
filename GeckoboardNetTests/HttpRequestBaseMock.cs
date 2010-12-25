// (C) 2011 Martin Wawrusch
//
// http://twitter.com/martin_sunset
// http://about.me/martinw
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Specialized;
using System.Web;

namespace GeckoboardNetTests
{
    /// <summary>
    /// Implements a fake HttpRequestBase object that we use for testing.
    /// </summary>
    internal class HttpRequestBaseMock : HttpRequestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see>HttpRequestBaseMock</see> class.
        /// </summary>
        /// <param name="authorizationHeader">A string representing the Basic... part of the authorization header. Will be omitted if null, empty or whitespace.</param>
        /// <param name="format">The format parameter as part of the post section. Will be omitted if null, empty or whitespace only.</param>
        /// <param name="type">The type parameter as part of the post section. Will be omitted if null, empty or whitespace only.</param>
        public HttpRequestBaseMock(string authorizationHeader, string format, string type)
        {
            _InitAuthorizationHeader(authorizationHeader);

            if (!string.IsNullOrWhiteSpace(format))
            {
                _Form.Add("format", format);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                _Form.Add("type", type);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see>HttpRequestBaseMock</see> class.
        /// </summary>
        /// <param name="authorizationHeader">A string representing the Basic... part of the authorization header. Will be omitted if null, empty or whitespace.</param>
        //public HttpRequestBaseMock(string authorizationHeader, IEnumerable<Tuple<string, string>> posts)
        //{
        //    _InitAuthorizationHeader(authorizationHeader);
        //    if (posts != null)
        //    {
        //        foreach (var x in posts)
        //        {
        //            _Form.Add(x.Item1, x.Item2);
        //        }
        //    }
        //}

        private void _InitAuthorizationHeader(string authorizationHeader)
        {
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                _Headers.Add("Authorization", authorizationHeader);
            }
        }

        NameValueCollection _Headers = new NameValueCollection();
        NameValueCollection _Form = new NameValueCollection();

        public override NameValueCollection Headers
        {
            get
            {
                return _Headers;
            }
        }

        public override NameValueCollection Form
        {
            get
            {
                return _Form;
            }
        }

        public override NameValueCollection Params
        {
            get
            {
                var res = new NameValueCollection(_Headers);
                res.Add(_Form);
                return res;
            }
        }
    }
}