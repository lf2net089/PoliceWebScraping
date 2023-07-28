using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliceWebScraping
{
    public class AzureResponse
    {
        public class Word
        {
            public string content { get; set; }
            // Other properties can be defined as needed
        }

        public class Page
        {
            public List<Word> words { get; set; }
        }

        public class ReadResult
        {
            public List<Page> pages { get; set; }
        }

        public class ResponseJson
        {
            public ReadResult readResult { get; set; }
        }

    }
}
