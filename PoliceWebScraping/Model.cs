
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliceWebScraping
{
    public class WebPageInfo
    {
        public string Title { get; }
        public string Url { get; }
        public string HtmlContent { get; }

        public WebPageInfo(string title, string url, string htmlContent)
        {
            Title = title;
            Url = url;
            HtmlContent = htmlContent;
        }
    }
    public class InputElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public byte[] Image { get; set; }
        public string TagName { get; set; }
        public string Placeholder { get; set; }
    }
}
