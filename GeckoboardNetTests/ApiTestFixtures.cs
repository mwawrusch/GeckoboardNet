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

using System.Collections.Generic;
using System.Web;
using GeckoboardNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeckoboardNetTests
{
    [TestClass]
    public class ApiTestFixtures
    {
        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ThrowBecauseInvalidApiKey()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "json", "4");
            var t = new GeckoboardService(request,
                (a, b) => new List<DataItemText> { new DataItemText { type = 1, text = "blah" } },
                a => a == "notavalidapikey" // <--
            );

            t.Result();
        }

        [TestMethod]
        public void ThisIsAValidApiKey()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "json", "4");
            var t = new GeckoboardService(request,
                (a, b) => new List<DataItemText> { new DataItemText { type = 1, text = "blah" } },
                a => a == FixtureConstants.Authorization_Test123_Source // <--
            );

            t.Result();
        }

        [TestMethod]
        public void ApIKeyValidationOmittedShouldPass()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "json", "4");
            var t = new GeckoboardService(request,
                (a, b) => new List<DataItemText> { new DataItemText { type = 1, text = "blah" } },
                null // <--
            );

            t.Result();
        }
    }
}