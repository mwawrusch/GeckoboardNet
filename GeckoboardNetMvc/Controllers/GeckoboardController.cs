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

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using GeckoboardNet;

namespace GeckoboardNetMvc.Controllers
{
    // Add this when running in Mvc 3.0 release. Has been renamed in Mvc 3.0 RC 2 from ControllerSessionState
    // [SessionState(SessionStateBehavior.Disabled)]
    public class GeckoboardController : Controller
    {
        /// <summary>
        /// Sample widget that returns some data for each widget type.
        /// </summary>
        /// <returns></returns>
        ///
        public ActionResult TestWidget()
        {
            return _TestWidget(false);
        }

        /// <summary>
        /// Sample widget that returns some data for each widget type. requires ApiKey 123
        /// </summary>
        /// <returns></returns>
        public ActionResult TestWidgetApi()
        {
            return _TestWidget(true);
        }

        /// <summary>
        /// Test that returns the API Key as the text of the first item.
        /// </summary>
        /// <returns></returns>
        public ActionResult TestWidgetApi2()
        {
            return new GeckoboardService(Request, (apiKey, widgetType) =>
                new List<DataItem> { new DataItem { @value = 123, text = apiKey }, new DataItem { @value = 238, text = "" } }
            ).Result();
        }

        private ActionResult _TestWidget(bool testApi)
        {
            return new GeckoboardService(Request, (apiKey, widgetType) =>
            {
                switch (widgetType)
                {
                    case WidgetType.NumberAndSecondaryStat:
                        return new List<DataItem> { new DataItem { @value = 123, text = string.Empty }, new DataItem { @value = 238, text = "" } };
                    case WidgetType.RagColumnAndNumbers:
                        return new List<DataItem> { new DataItem { @value = 123, text = "Text1" }, new DataItem { @value = 238, text = "Text2" }, new DataItem { @value = 456, text = "Text3" } };
                    case WidgetType.RagNumbersOnly:
                        return new List<DataItem> { new DataItem { @value = 123, text = "Text1" }, new DataItem { @value = 238, text = "Text2" }, new DataItem { @value = 456, text = "Text3" } };
                    case WidgetType.Text:
                        return new List<DataItemText> { new DataItemText { type = 1, text = "This is line 1" }, new DataItemText { type = 2, text = "This is line 2" }, new DataItemText { type = 3, text = "This is line 3" } };
                    case WidgetType.ChartPie:
                        return new List<DataItemPieChart> { new DataItemPieChart { @value = 100, label = "May", colour = "FFFF10AA" }, new DataItemPieChart { @value = 160, label = "June", colour = "FFAA0AAA" }, new DataItemPieChart { @value = 300, label = "July", colour = "FF5505AA" }, new DataItemPieChart { @value = 140, label = "August", colour = "FF0000AA" } };
                    case WidgetType.ChartLine:
                        return new DataLineChart
                        {
                            item = new List<decimal> { 12.3M, 2.3M, 10M, 15M, 15M, 13M, 12.1M, 9.8M, 12.3M, 2.3M, 10M, 12M, 15M, 13M, 12.1M, 9.8M, 11M, 16M, 15M, 13M, 10M, 7M },
                            settings = new DataLineChartSettings
                            {
                                axisx = new List<string> { "Jun", "Jul", "Aug" },
                                axisy = new List<string> { "Min", "Max" },
                                colour = "ff9900"
                            }
                        };
                    case WidgetType.GeckOMeter:
                        return new DataGeckometer { item = 23M, min = new DataItem { @value = 10M, text = "Min visitors" }, max = new DataItem { @value = 30M, text = "Max visitors" } };
                }
                throw new NotSupportedException(); // Compiler bug; the compiler should determine that all options are handled in the switch statement above and that it could never come here. In that case it would create a build error when we add a new enum type, which is now only catchable at runtime.
            }, a => !testApi || a == "123").Result();
        }
    }
}