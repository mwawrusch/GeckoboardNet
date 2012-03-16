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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace GeckoboardNet
{
    /// <summary>
    /// Handles an incoming Geckoboard request.
    /// </summary>
    public class GeckoboardService
    {
        private const string BasicAuthentication = "basic "; // note the space at the end.
        private const string AuthorizationHeader = "Authorization";
        private const string ParamFormat = "format";
        private const string FormatXml = "1";
        private const string FormatJson = "0";
        private const string FormatXmlAlt = "xml";
        private const string FormatJsonAlt = "json";
        private const string ParamType = "type";
        private const string ErrorMissingFormat = "Missing format parameter which must be either xml or json.";
        private const string ErrorMissingType = "Missing type parameter which must be 1,2,3 or 4.";
        private const string ErrorNoApiKey = "No api key present.";
        private const string ErrorInvalidApiKey = "Invalid API Key.";
        private const string ErrorCouldNotCompleteRequest = "Could not complete the request.";

        private static string _ExtractParam(HttpRequestBase request, string paramName)
        {
            foreach (var x in request.Params.AllKeys)
            {
                if (x.Equals(paramName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return request.Params[x] ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeckoboardService"/> class.
        /// </summary>
        /// <param name="request">The request. Must not be null.</param>
        /// <param name="execute">A function that is executed and must return the appropriate result for the widget type. Must not be null.</param>
        /// <param name="isValidApiKey">A function that is exectued before the result is processed and hat triggers a 403 if it returns false. Input parameter is the extracted api key. If not present it is assumed to be true. Can be null.</param>
        /// <exception cref="HttpException">Thrown when the request does not contain the format and type specifier as defined in the geckoboard api.</exception>
        public GeckoboardService(HttpRequestBase request, Func<string, WidgetType, object> execute, Func<string, bool> isValidApiKey = null)
        {
            Contract.Requires(request != null);
            Contract.Requires(execute != null);

            Execute = execute;
            IsValidApiKey = isValidApiKey;

            var formatRaw = _ExtractParam(request, ParamFormat);

            if (formatRaw.Equals(FormatXml, StringComparison.InvariantCultureIgnoreCase) ||
                formatRaw.Equals(FormatXmlAlt, StringComparison.InvariantCultureIgnoreCase))
                ResponseFormat = ResponseFormat.Xml;
            else if (formatRaw.Equals(FormatJson, StringComparison.InvariantCultureIgnoreCase) ||
                     formatRaw.Equals(FormatJsonAlt, StringComparison.InvariantCultureIgnoreCase))
                ResponseFormat = ResponseFormat.Json;
            //          else if (string.IsNullOrWhiteSpace(formatRaw))
            else
                ResponseFormat = ResponseFormat.Json;
            //else
            //    throw new HttpException(400, ErrorMissingType);
            // default is json

            var typeRaw = 0;

            if (!int.TryParse(_ExtractParam(request, ParamType), out typeRaw) ||
                typeRaw < (int)WidgetType.NumberAndSecondaryStat ||
                typeRaw > Enum.GetValues(typeof(WidgetType)).Length)
                throw new HttpException(400, ErrorMissingType);

            WidgetType = (WidgetType)typeRaw;

            // api key is optional, so if we have no basic authentication we ignore it.
            ApiKey = string.Empty;

            var authHeader = request.Headers[AuthorizationHeader] ?? string.Empty;
            if (authHeader.StartsWith(BasicAuthentication, StringComparison.InvariantCultureIgnoreCase))
            {
                try // geckoboard api is not documented well these days, so we just wing it.
                {
                    var userNameAndPassword = Encoding.Default.GetString(Convert.FromBase64String(authHeader.Substring(BasicAuthentication.Length)));
                    var parts = userNameAndPassword.Split(':');

                    ApiKey = (parts.Length > 0 ? parts[0] : userNameAndPassword) ?? string.Empty;
                }
                catch (Exception)
                {
                }
            }
        }

        public bool TraceExecuteExceptionsAsErrors { get; set; }

        /// <summary>
        /// The api key as it is passed from geckoboard.
        /// </summary>
        public string ApiKey
        {
            get;
            private set;
        }

        public ResponseFormat ResponseFormat
        {
            get;
            private set;
        }

        public WidgetType WidgetType
        {
            get;
            private set;
        }

        public Func<string, WidgetType, object> Execute
        {
            get;
            set;
        }

        /// <summary>
        /// A function that is exectued before the result is processed and hat triggers a 403 if it returns false. Input parameter is the extracted api key. If not present it is assumed to be true. Can be null.
        /// </summary>
        public Func<string, bool> IsValidApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a result object to be returned within your MVC controller.
        /// </summary>
        /// <returns>An result containing either json or xml. Never null.</returns>
        public ActionResult Result()
        {
            Contract.Ensures(Contract.Result<ActionResult>() != null);

            if (IsValidApiKey != null && !IsValidApiKey(ApiKey ?? String.Empty))
                throw new HttpException(403, ErrorInvalidApiKey);

            object result = null;

            try
            {
                result = Execute(ApiKey, WidgetType);
            }
            catch (Exception ex)
            {
                if (TraceExecuteExceptionsAsErrors)
                {
                    Trace.TraceError("Attempt to retrieve result for Geckoboard failed: {0}", ex.Message);
                }
                throw new HttpException(500, ErrorCouldNotCompleteRequest);
            }

            if (result == null)
                throw new HttpException(500, ErrorCouldNotCompleteRequest);

            if (result is IEnumerable)
            {
                var dh = new DataHelper();

                foreach (var x in (IEnumerable)result)
                {
                    dh.item.Add(x);
                }

                result = dh;
            }

            if (ResponseFormat == ResponseFormat.Json)
            {
                return new JsonResult
                {
                    Data = result,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet // we want to be able to debug this with gets too.
                };
            }
            else
            {
                string content;
                using (var stream = new MemoryStream(500))
                {
                    using (var xmlWriter =
                        XmlWriter.Create(stream,
                            new XmlWriterSettings
                                {
                                    OmitXmlDeclaration = false,
                                    Encoding = Encoding.UTF8,
                                    Indent = true
                                }))
                    {
                        new XmlSerializer(result.GetType())
                            .Serialize(xmlWriter, result);
                    }

                    content = Encoding.UTF8.GetString(stream.ToArray());
                }

                return new ContentResult
                {
                    Content = content,
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "text/xml"
                };
            }
        }
    }

    /// <summary>
    /// The response format requested.
    /// </summary>
    public enum ResponseFormat
    {
        Json,
        Xml
    }

    /// <summary>
    /// The type of widget requested.
    /// </summary>
    public enum WidgetType
    {
        // Numbers must correspond with the geckoboard types.
        NumberAndSecondaryStat = 1, // Important, never remove the 1
        RagColumnAndNumbers,
        RagNumbersOnly,
        Text,
        ChartPie,
        ChartLine,
        GeckOMeter
    }

    public class DataItemPieChart
    {
        /// <summary>
        /// The value of this data point
        /// </summary>
        public decimal @value { get; set; }

        /// <summary>
        /// The label of this data point
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// The color value which must be in 6 or 8 character hex notation, as so: RRGGBBAA
        /// </summary>
        /// <remarks>
        /// Spelling is english for now.
        /// </remarks>
        public string colour { get; set; }
    }

    public class DataItem
    {
        /// <summary>
        /// The label of this data point
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// The value of this data point
        /// </summary>
        public decimal @value { get; set; }
    }

    public class DataItemText
    {
        /// <summary>
        /// The type of text entry, which can be 0=None (no corner icon), 1=Info (grey corner icon), 2=Alert (yellow corner icon)
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// The label of this data point which can be plain text or html.
        /// </summary>
        public string text { get; set; }

        // XmlCDataSection
    }

    public class DataGeckometer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGeckometer"/> class.
        /// </summary>
        public DataGeckometer()
        {
            //min = new DataItem[1];
            //max = new DataItem[1];

            //min[0] = new DataItem();
            //max[0] = new DataItem();
            min = new DataItem();
            max = new DataItem();
        }

        /// <summary>
        /// The value of this data point
        /// </summary>
        public decimal item { get; set; }

        /// <summary>
        /// The label of this data point
        /// </summary>
        public DataItem min { get; set; }

        public DataItem max { get; set; }
    }

    public class DataLineChart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGeckometer"/> class.
        /// </summary>
        public DataLineChart()
        {
            item = new List<decimal>();
        }

        public List<decimal> item { get; set; }

        public DataLineChartSettings settings { get; set; }
    }

    public class DataLineChartSettings
    {
        public DataLineChartSettings()
        {
            axisx = new List<string>();
            axisy = new List<string>();
        }

        public List<string> axisx { get; set; }

        public List<string> axisy { get; set; }

        public string colour { get; set; }
    }

    public class DataHelper
    {
        public DataHelper()
        {
            item = new List<object>();
        }

        public List<object> item { get; set; }
    }
}