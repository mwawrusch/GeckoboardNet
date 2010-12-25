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
using System.Web.Mvc;
using System.Web.Script.Serialization;
using GeckoboardNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeckoboardNetTests
{
    /// <summary>
    /// IMPORTANT: The test result checks depend on the order of the type definitions within the data item declarations.
    /// In a better world we would mock the actual execution of the results, but who cares...
    /// </summary>
    [TestClass]
    public class JsonSerializeFixtures
    {
        [TestMethod]
        public void TestValidForType1()
        {
            var res = _CreateJsonResult(WidgetType.NumberAndSecondaryStat, new List<DataItem> { new DataItem { @value = 123, text = string.Empty }, new DataItem { @value = 238, text = "" } });

            Assert.AreEqual("{\"item\":[{\"text\":\"\",\"value\":123},{\"text\":\"\",\"value\":238}]}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType2()
        {
            var res = _CreateJsonResult(WidgetType.RagColumnAndNumbers, new List<DataItem> { new DataItem { @value = 123, text = "Text1" }, new DataItem { @value = 238, text = "Text2" }, new DataItem { @value = 456, text = "Text3" } });

            Assert.AreEqual("{\"item\":[{\"text\":\"Text1\",\"value\":123},{\"text\":\"Text2\",\"value\":238},{\"text\":\"Text3\",\"value\":456}]}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType3()
        {
            var res = _CreateJsonResult(WidgetType.RagNumbersOnly, new List<DataItem> { new DataItem { @value = 123, text = "Text1" }, new DataItem { @value = 238, text = "Text2" }, new DataItem { @value = 456, text = "Text3" } });

            Assert.AreEqual("{\"item\":[{\"text\":\"Text1\",\"value\":123},{\"text\":\"Text2\",\"value\":238},{\"text\":\"Text3\",\"value\":456}]}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType4()
        {
            var res = _CreateJsonResult(WidgetType.Text, new List<DataItemText> { new DataItemText { type = 1, text = "blah" } });

            Assert.AreEqual("{\"item\":[{\"type\":1,\"text\":\"blah\"}]}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType5()
        {
            var res = _CreateJsonResult(WidgetType.ChartPie, new List<DataItemPieChart> { new DataItemPieChart { @value = 100, label = "May", colour = "FFFF10AA" }, new DataItemPieChart { @value = 160, label = "June", colour = "FFAA0AAA" }, new DataItemPieChart { @value = 300, label = "July", colour = "FF5505AA" }, new DataItemPieChart { @value = 140, label = "August", colour = "FF0000AA" } });

            Assert.AreEqual("{\"item\":[{\"value\":100,\"label\":\"May\",\"colour\":\"FFFF10AA\"},{\"value\":160,\"label\":\"June\",\"colour\":\"FFAA0AAA\"},{\"value\":300,\"label\":\"July\",\"colour\":\"FF5505AA\"},{\"value\":140,\"label\":\"August\",\"colour\":\"FF0000AA\"}]}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType6()
        {
            var res = _CreateJsonResult(WidgetType.ChartLine, new DataLineChart
            {
                item = new List<decimal> { 12.3M, 2.3M, 10M, 15M, 15M, 13M, 12.1M, 9.8M, 12.3M, 2.3M, 10M, 12M, 15M, 13M, 12.1M, 9.8M, 11M, 16M, 15M, 13M, 10M, 7M },
                settings = new DataLineChartSettings
                {
                    axisx = new List<string> { "Jun", "Jul", "Aug" },
                    axisy = new List<string> { "Min", "Max" },
                    colour = "ff9900"
                }
            });

            Assert.AreEqual("{\"item\":[12.3,2.3,10,15,15,13,12.1,9.8,12.3,2.3,10,12,15,13,12.1,9.8,11,16,15,13,10,7],\"settings\":{\"axisx\":[\"Jun\",\"Jul\",\"Aug\"],\"axisy\":[\"Min\",\"Max\"],\"colour\":\"ff9900\"}}", _DeserializeResult(res));
        }

        [TestMethod]
        public void TestValidForType7()
        {
            //var res = _CreateJsonResult(WidgetType.GeckOMeter, new DataGeckometer() { item = 23M, min = new DataItem[] { new DataItem { @value = 10M, text = "Min visitors" } }, max = new DataItem[] { new DataItem { @value = 30M, text = "Max visitors" } } });
            var res = _CreateJsonResult(WidgetType.GeckOMeter, new DataGeckometer { item = 23M, min = new DataItem { @value = 10M, text = "Min visitors" }, max = new DataItem { @value = 30M, text = "Max visitors" } });

            Assert.AreEqual("{\"item\":23,\"min\":{\"text\":\"Min visitors\",\"value\":10},\"max\":{\"text\":\"Max visitors\",\"value\":30}}", _DeserializeResult(res));
        }

        private static string _DeserializeResult(JsonResult res)
        {
            // quick and dirty
            var serializer = new JavaScriptSerializer();
            var output = serializer.Serialize(res.Data);
            return output;
        }

        private static JsonResult _CreateJsonResult(WidgetType widgetType, object data)
        {
            var request = new HttpRequestBaseMock(FixtureConstants.Authorization_Test123, "json", ((int)widgetType).ToString());
            var t = new GeckoboardService(request,
                (a, b) => data,
                a => a == FixtureConstants.Authorization_Test123_Source
            );

            var res = t.Result() as JsonResult;

            Assert.IsNotNull(res);
            return res;
        }
    }
}