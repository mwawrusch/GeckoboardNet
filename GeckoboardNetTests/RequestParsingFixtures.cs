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

using System.Web;
using GeckoboardNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeckoboardNetTests
{
    [TestClass]
    public class RequestParsingFixtures
    {
        [TestMethod]
        public void IsAValidApiRequestWithXmlAndType2()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "xml", "2");
            var t = new GeckoboardService(request, (a, b) => null, a => true);

            Assert.AreEqual(FixtureConstants.Authorization_Test123_Source, t.ApiKey);
            Assert.AreEqual(ResponseFormat.Xml, t.ResponseFormat);
            Assert.AreEqual(WidgetType.RagColumnAndNumbers, t.WidgetType);
        }

        [TestMethod]
        public void IsAValidApiRequestWithJsonAndType4()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "json", "4");
            var t = new GeckoboardService(request, (a, b) => null, a => true);

            Assert.AreEqual(FixtureConstants.Authorization_Test123_Source, t.ApiKey);
            Assert.AreEqual(ResponseFormat.Json, t.ResponseFormat);
            Assert.AreEqual(WidgetType.Text, t.WidgetType);
        }

        // for some reason the format passed is not what we expect. We therefore default to Json
        // which renders this test obsolete
        //[TestMethod]
        //[ExpectedException(typeof(HttpException))]
        //public void ThrowBecauseOfInvalidFormat()
        //{
        //    var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "foobar", "4");
        //    var t = new GeckoboardService(request, (a, b) => null, a => true);
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ThrowBecauseOfNoType()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "xml", null);
            var t = new GeckoboardService(request, (a, b) => null, a => true);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ThrowBecauseOfInvalidType()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "xml", "bar");
            var t = new GeckoboardService(request, (a, b) => null, a => true);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ThrowBecauseOfOutOfRangeType()
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "xml", "0");
            var t = new GeckoboardService(request, (a, b) => null, a => true);
        }
    }
}