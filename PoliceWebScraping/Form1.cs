using AngleSharp.Html.Parser;
using F23.StringSimilarity;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using static PoliceWebScraping.ImageHelper;
using ComboBox = System.Windows.Forms.ComboBox;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using ListBox = System.Windows.Forms.ListBox;
using RadioButton = System.Windows.Forms.RadioButton;
using F23;

namespace PoliceWebScraping
{
    public partial class Form1 : Form
    {
        private float splitRatio = 0.7f;
        static string contentCaptcha = string.Empty;
        private List<WebPageInfo> pageInfoList = new List<WebPageInfo>();
        private HttpClientHandler handler = new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer() // Assuming you have a CookieContainer object with your cookies
        };
        private FileInfo xmlFileInfo = new FileInfo("Options.xml");
        private FileInfo htmlInputElementJson = new FileInfo("InputElement.json");
        private HttpClient httpClient;
        private bool isDropdownFormOpen = false;
        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
        }
        private async void btStep_ClickAsync(object sender, EventArgs e)
        {

            string javascriptCode = @"
                    var links = document.querySelectorAll('a[href=""traffic_write.jsp""]');
                    for (var i = 0; i < links.length; i++) {
                        links[i].click();
                    }";

            // 執行JavaScript代碼
            await webView21.CoreWebView2.ExecuteScriptAsync(javascriptCode);
        }
        private async void InitializeWebView()
        {
            webView21.Dock = DockStyle.Fill;
            // Initialize HttpClient with handler
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // 等待WebView2控制項初始化完成
            await webView21.EnsureCoreWebView2Async();
            GetElementsAsync(); ;

            // 設定網頁載入完成的事件處理器
            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompletedAsync; ;
            webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // 導航到目標網頁
            webView21.Source = new Uri("https://suggest.police.taichung.gov.tw/traffic/traffic_write.jsp");

        }
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string dataUrl = e.TryGetWebMessageAsString();
                string base64Data = dataUrl.Substring(22);
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = System.Drawing.Image.FromStream(ms);
                    pbCaptcha.Image = image;
                    pbCaptcha.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or show an error message.
                // For now, let's just log the error to the console.
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Return or do any other necessary error handling here.
                return;
            }
        }

        private async Task RefreshWebViewAsync()
        {
            // Wait for the WebView to be initialized and ensure that CoreWebView2 is available
            if (webView21.CoreWebView2 == null)
            {
                await webView21.EnsureCoreWebView2Async();
            }

            // Refresh the WebView page
            webView21.Reload();
        }
        private async void GetElementsAsync()
        {
            webView21.NavigationCompleted += async (sender, e) =>
            {
                var inputElementsJson = await webView21.CoreWebView2.ExecuteScriptAsync(@"
                var inputElements = Array.from(document.querySelectorAll('input'));
                var inputs = inputElements.filter(function(element) {
                    return element.value !== '' && element.value !== undefined;
                });
                inputs.map(function(element) {
                    return { id: element.id, value: element.value };
                });");

                var inputElements = JArray.Parse(inputElementsJson).Select(x => new InputElement
                {
                    Id = x["id"].Value<string>(),
                    Value = x["value"].Value<string>()
                }).ToList();

                foreach (var inputElement in inputElements)
                {
                    Console.WriteLine($"ID: {inputElement.Id}, Value: {inputElement.Value}");
                }
            };
        }
        private async void CoreWebView2_NavigationCompletedAsync(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var html = await webView21.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            var htmlDecoded = WebUtility.HtmlDecode(html);
            // 執行點擊動作的JavaScript代碼
            string title = await webView21.CoreWebView2.ExecuteScriptAsync("document.title");
            string url = webView21.Source?.ToString();
            // 建立WebPageInfo物件並新增到清單中
            WebPageInfo pageInfo = new WebPageInfo(title, url, htmlDecoded);
            pageInfoList.Add(pageInfo);
            if (pageInfoList.Count == 2)
            {
                // Execute JavaScript code to get the captcha image
                string jsScript = @"
                    try {
                        let img = document.getElementById('captcha_pic');
                        let canvas = document.createElement('canvas');
                        let ctx = canvas.getContext('2d');
                        ctx.drawImage(img, 0, 0);
                        let dataUrl = canvas.toDataURL('image/png');
                        window.chrome.webview.postMessage(dataUrl);
                    } catch (error) {
                        window.chrome.webview.postMessage('Error: ' + error.message);
                    }";

                await webView21.CoreWebView2.ExecuteScriptAsync(jsScript);
                btSaveIni_ClickAsync(sender, null);
            }
        }
        private async void btSaveIni_ClickAsync(object sender, EventArgs e)
        {
            var azureAccount = new AzureAccount
            {
                OCRKey = ConfigurationManager.AppSettings["AzureOCRKey"],
                VisionEndpoint = ConfigurationManager.AppSettings["VISION_ENDPOINT"]
            };
            var imgurAccount = new ImgurAccount
            {
                ID = ConfigurationManager.AppSettings["ImgurID"],
                Secret = ConfigurationManager.AppSettings["ImgurP"],
                AccessToken = ConfigurationManager.AppSettings["Imguraccess_token"],
                RefreshToken = ConfigurationManager.AppSettings["ImgurRefreshToken"],
                AccountID = ConfigurationManager.AppSettings["ImgurAccountId"],
                ExpiresIn = ConfigurationManager.AppSettings["ImgurExpiresIn"]
            };
            await webView21.EnsureCoreWebView2Async();
            contentCaptcha = await CallComputerVisionApi(azureAccount, imgurAccount, pbCaptcha.Image);
            UpdateText(txtpbCaptcha, contentCaptcha);
            await Task.Delay(3000);
            var htmlContent = await webView21.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
            string jsonHtml = htmlContent.ToString().Trim('"');
            string html = System.Text.RegularExpressions.Regex.Unescape(jsonHtml);
            string decodedHtml = System.Web.HttpUtility.HtmlDecode(html);
            if (string.IsNullOrEmpty(htmlContent))
                return;

            var parser = new HtmlParser();
            var document = parser.ParseDocument(decodedHtml);
            var documentAsync = await parser.ParseDocumentAsync(html);


            var selectElements = await webView21.CoreWebView2.ExecuteScriptAsync(@"
            var selectElements = Array.from(document.querySelectorAll('select'));
            selectElements.map(function(element) {
                return { id: element.id, name: element.name, value: element.value };
            });
            ");

            var inputAllElementss = document.QuerySelectorAll("input");
            var selectAllElementss = document.QuerySelectorAll("select");
            IEnumerable<InputElement> selectNodes = document.QuerySelectorAll("select")
            .Select(x => new InputElement
            {
                Id = x.GetAttribute("id"),
                Name = x.GetAttribute("name"),
                Value = x.GetAttribute("value")
            });

            await ParseHtmlToJSONAsync(html);
            var selectNodesJson = JArray.FromObject(selectNodes);
            List<string> GetValuesForKey_Qclass = new List<string>();
            if (!xmlFileInfo.Exists || Math.Abs(xmlFileInfo.LastWriteTime.Subtract(DateTime.Now).TotalDays) >= 10)
            {
                Dictionary<string, List<string>> selectableOptions = new Dictionary<string, List<string>>();

                foreach (var selectNode in selectNodes)
                {
                    string selectId = selectNode.Id;
                    string selectName = selectNode.Name;
                    if (string.IsNullOrEmpty(selectId) && !string.IsNullOrEmpty(selectName))
                    {
                        if (selectName == "cityarea" || selectName == "street")
                        {
                            string scriptOptions = $@"
                            Array.from(document.querySelector('select[name={selectName}]').options)
                                 .map(option => option.value);
                        ";
                            string optionsResult = await webView21.CoreWebView2.ExecuteScriptAsync(scriptOptions);
                            List<string> options = JsonConvert.DeserializeObject<List<string>>(optionsResult);

                            foreach (var option in options)
                            {
                                Console.WriteLine($"{selectName} Option: {option}");
                            }

                            if (selectName == "cityarea")
                            {
                                foreach (var cityArea in options)
                                {
                                    string selectCityAreaScript = $"document.querySelector('select[name=cityarea]').value = '{cityArea}';";
                                    await webView21.CoreWebView2.ExecuteScriptAsync(selectCityAreaScript);
                                    string triggerCityAreaChangeScript = $"document.querySelector('select[name=cityarea]').dispatchEvent(new Event('change'));";
                                    await webView21.CoreWebView2.ExecuteScriptAsync(triggerCityAreaChangeScript);
                                    await Task.Delay(1000);
                                    string scriptStreetOptions = @"
                                    Array.from(document.querySelector('select[name=street]').options)
                                    .map(option => option.value);";
                                    string streetOptionsResult = await webView21.CoreWebView2.ExecuteScriptAsync(scriptStreetOptions);
                                    List<string> streetOptions = JsonConvert.DeserializeObject<List<string>>(streetOptionsResult);

                                    foreach (var street in streetOptions)
                                    {
                                        Console.WriteLine($"Street Option: {street}");
                                    }
                                    streetOptions = streetOptions.Where(s => s.Trim().Length > 0).ToList();
                                    selectableOptions[cityArea.ToString()] = streetOptions;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 觸發下拉式選單彈出選單內容
                        string script = $"document.getElementById('{selectId}').click();";
                        await webView21.CoreWebView2.ExecuteScriptAsync(script);

                        // 等待一定時間以確保選單內容已經彈出
                        await Task.Delay(1000);

                        // 獲取下拉式選單的選項內容
                        string optionsScript = $"Array.from(document.getElementById('{selectId}').options).map(function(option) {{ return option.value; }});";
                        var optionsResult = await webView21.CoreWebView2.ExecuteScriptAsync(optionsScript);
                        var options = JsonConvert.DeserializeObject<List<string>>(optionsResult);
                        if (options == null)
                            continue;
                        options = options.Where(s => s.Trim().Length > 0).ToList();
                        if (selectId == "qclass")
                            UpdateComboBox(cbViolationSelector, options);
                        var regulations = JsonConvert.SerializeObject(options);
                        //regulations = Regex.Replace(regulations, "\\(測試中\\)|…|道交[0-9之第-]+|[0-9之-]+", "");
                        string[] delimiters = { "\",\"", "[\"", "\"]" };
                        var parts = regulations.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            parts[i] = parts[i].Trim();
                        }
                        selectableOptions[selectName] = parts.ToList();
                    }
                }

                XmlHelper.SaveSelectableOptions(xmlFileInfo.FullName, selectableOptions);
            }
            else
            {
                var loadXmlOptions = XmlHelper.LoadSelectableOptions(xmlFileInfo.FullName);
                GetValuesForKey_Qclass = XmlHelper.GetValuesForKey("qclass", loadXmlOptions);
                UpdateComboBox(cbViolationSelector, GetValuesForKey_Qclass);
            }

            ReportInfo reportInfo = new ReportInfo
            {
                Name = "陳小廷",
                Gender = "male",
                IsForeigner = "taiwan",
                Sub = "S123456789",
                Address = "台中市龍井區虛構大樓999號",
                LiaisonTel = "0900000000",
                Email = "lf2net089@gmail.com",
                Captcha = contentCaptcha,
                DetailContent = String.Join(",", txtDescription.Text.Split(Environment.NewLine)),
                ViolationDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                MapAddress = @"434台灣台中市龍井區臺灣大道五段301號",
            };
            if (GetValuesForKey_Qclass.Count * reportInfo.DetailContent.Length > 0)
            {
                var APIanswer = await callOpenAIAsync(GetValuesForKey_Qclass, reportInfo.DetailContent);
                var AnswerSelector = SelectRegulations(GetValuesForKey_Qclass, APIanswer);
                if (AnswerSelector >= 0)
                {
                    reportInfo.QClass = AnswerSelector.ToString();
                    SelectComboBoxItem(cbViolationSelector, AnswerSelector);
                }
            }
            Dictionary<string, string> fieldValues = AutoFill(reportInfo);
            await SetFieldValuesInWebPageAsync(fieldValues);
            string json = await ParseHtmlAndModifyFieldsAsync(html, fieldValues);

        }
        public Dictionary<string, string> AutoFill(ReportInfo reportInfo)
        {
            // Handle the media files
            if (!string.IsNullOrEmpty(fbdMediaSelect.SelectedPath))
            {
                string[] mediaFiles = Directory.GetFiles(fbdMediaSelect.SelectedPath);

                List<string> videoFiles = new List<string>();
                List<string> imageFiles = new List<string>();

                foreach (var file in mediaFiles)
                {
                    if (videoExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    {
                        videoFiles.Add(file);
                    }
                    else if (imageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    {
                        imageFiles.Add(file);
                    }
                }

                AssignFileNameIfEmpty(reportInfo, videoFiles, 3);
                AssignFileNameIfEmpty(reportInfo, imageFiles, 3);
            }

            if (groupBox1.Controls.Count > 0)
            {
                var selectedRadioButton = groupBox1.Controls.OfType<RadioButton>().FirstOrDefault(radio => radio.Checked);
                if (selectedRadioButton != null)
                {
                    var radioTextParts = selectedRadioButton.Text.Split(',');
                    if (radioTextParts.Length >= 3)
                    {
                        var licensePlateNumberParts = radioTextParts[0].Split('-');
                        if (licensePlateNumberParts.Length == 2)
                        {
                            reportInfo.LicenseNumber2 = licensePlateNumberParts[0].Trim();
                            reportInfo.LicenseNumber3 = licensePlateNumberParts[1].Trim();
                        }
                    }
                    if (radioTextParts.Length >= 4)
                    {
                        reportInfo.ViolationDateTime = radioTextParts[3];
                    }
                }
            }

            // Set the values from reportInfo to the fieldValues dictionary
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();

            Type reportInfoType = typeof(ReportInfo);
            PropertyInfo[] properties = reportInfoType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name.ToLower();
                object propertyValue = property.GetValue(reportInfo);

                if (propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString()))
                {
                    fieldValues[propertyName] = propertyValue.ToString();
                }
            }

            return fieldValues;
        }
        private void AssignFileNameIfEmpty(ReportInfo reportInfo, List<string> files, int maxCount)
        {
            int count = 0;
            foreach (var file in files)
            {
                bool flag = false;
                do
                {
                    count++;
                    string fileName = $"FileName{count}";
                    string currentValue = typeof(ReportInfo).GetProperty(fileName)?.GetValue(reportInfo) as string;

                    if (string.IsNullOrEmpty(currentValue))
                    {
                        typeof(ReportInfo).GetProperty(fileName)?.SetValue(reportInfo, file);
                        flag = true;
                    }
                    if (count >= maxCount)
                    {
                        break;
                    }
                } while (!flag);
            }
        }
        public async Task SetFieldValuesInWebPageAsync(Dictionary<string, string> fieldValues)
        {
            foreach (var fieldValue in fieldValues)
            {
                string script = string.Empty;
                switch (fieldValue.Key)
                {
                    case "qclass":
                        int selectedIndex;
                        if (int.TryParse(fieldValue.Value, out selectedIndex))
                        {
                            script = $"document.getElementById('{fieldValue.Key}').selectedIndex = {selectedIndex};";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);
                        }
                        break;
                    case "violationdatetime":
                        DateTime datetime;
                        if (DateTime.TryParseExact(fieldValue.Value.Trim(), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime))
                        {
                            // Simulate focusing on the violationdatetime field to trigger the datepicker
                            string focusScript = @"
                            var violationDatetimeField = document.getElementById('violationdatetime');
                            violationDatetimeField.focus(); // Set focus on the field
                            ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(focusScript);

                            // Simulate triggering the datepicker
                            string triggerScript = @"
                            var violationDatetimeField = document.getElementById('violationdatetime');
                            if (typeof jQuery !== 'undefined' && jQuery.fn.datetimepicker) {
                                jQuery(violationDatetimeField).datetimepicker('show'); // Trigger the datepicker
                            }
                            ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(triggerScript);

                            // Wait for the datepicker to appear before proceeding with the selection
                            await Task.Delay(2000); // Adjust the delay as needed

                            // Simulate selecting the date
                            string selectDateScript = $@"
                                    var violationDatetimeField = document.getElementById('violationdatetime');
                                    if (typeof jQuery !== 'undefined' && jQuery.fn.datepicker) {{
                                        var datepicker = jQuery(violationDatetimeField).datepicker('widget');
                                        datepicker.find('td[data-handler=selectDay] a:contains({datetime.Day})').click();
                                    }}
                                    ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(selectDateScript);
                            await Task.Delay(100);
                            // Simulate selecting the hour
                            string selectHourScript = $@"
                            var hourSelect = document.querySelector('select[data-unit=hour]');
                            hourSelect.value = '{datetime.Hour.ToString("00")}'; // Set the desired hour value
                            hourSelect.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(selectHourScript);
                            await Task.Delay(100);
                            // Simulate selecting the minute
                            string selectMinuteScript = $@"
                            var minuteSelect = document.querySelector('select[data-unit=minute]');
                            minuteSelect.value = '{datetime.Minute.ToString("00")}'; // Set the desired minute value
                            minuteSelect.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(selectMinuteScript);
                            await Task.Delay(100);
                            string confirmScript = @"
                            var confirmButton = document.querySelector('.ui-datepicker-close');
                            confirmButton.click();
                            ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(confirmScript);
                            await Task.Delay(100);
                        }
                        break;

                    case "licensenumber1":
                        if (fieldValue.Value == "一般車牌")
                        {
                            script = $"document.getElementById('licensenumber2').value = '';";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);
                            script = $"document.getElementById('licensenumber3').value = '';";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);

                            script = $"document.getElementById('licensenumber4').disabled = false;";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);
                        }
                        else
                        {
                            script = $"document.getElementById('licensenumber4').value = '';";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);

                            script = $"document.getElementById('licensenumber4').disabled = true;";
                            await webView21.CoreWebView2.ExecuteScriptAsync(script);
                        }
                        break;
                    case "filename1":
                    case "filename2":
                    case "filename3":
                    case "filename4":
                        string fileInputId = fieldValue.Key; // File input field ID
                        await SelectFileAsync(fileInputId, fieldValue.Value);

                        break;
                    case "mapaddress":
                        if (!string.IsNullOrEmpty(fieldValue.Value))
                        {
                            string selectMapFlagScript = @"
                                    document.getElementById('mapflag').checked = true;
                                    chg();
                                ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(selectMapFlagScript);
                            string fillMapAddressScript = $@"
                                    document.getElementById('mapaddress').value = '{fieldValue.Value}';
                                ";
                            await webView21.CoreWebView2.ExecuteScriptAsync(fillMapAddressScript);
                        }
                        break;
                    default:

                        script = $"document.getElementById('{fieldValue.Key.ToLower()}').value = '{fieldValue.Value}';";
                        await webView21.CoreWebView2.ExecuteScriptAsync(script);
                        break;
                }
            }
        }
        public async Task SelectFileAsync(string fileInputId, string filePath)
        {
            string formTitle = "Whistleblower Management System";
            // Simulate click on the file input field
            string clickFileInputScript = $@"
                var fileInput = document.getElementById('{fileInputId}');
                fileInput.click();
            ";
            await webView21.CoreWebView2.ExecuteScriptAsync(clickFileInputScript);
            await Task.Delay(1000); // Adjust the delay as needed

            using (var automation = new UIA3Automation())
            {
                var desktop = automation.GetDesktop();
                var mainWindow = desktop.FindFirstChild(cf => cf.ByName(formTitle)).AsWindow();
                if (mainWindow != null)
                {
                    var dialogWindowElement = mainWindow.FindFirstChild(cf => cf.ByName("開啟")).AsWindow();
                    if (dialogWindowElement != null)
                    {
                        await Task.Delay(2000);
                        var sssf = dialogWindowElement.Properties.Name.ValueOrDefault;
                        Console.WriteLine($"Window name: {sssf}");

                        var fileNameInput = dialogWindowElement.FindFirstChild(cf => cf.ByAutomationId(1090.ToString())).AsTextBox();
                        Clipboard.SetText(filePath);
                        SendKeys.SendWait("^v");
                        if (!fileNameInput.Text.Equals(filePath))
                        {
                            fileNameInput.Enter(filePath);
                        }
                        await Task.Delay(1000);
                    }
                    else
                    {
                        throw new Exception("Dialog window not found.");
                    }
                }
                else
                {
                    throw new Exception($"Main window ({formTitle}) not found.");
                }
            }
        }
        public async Task<string> ParseHtmlToJSONAsync(string html)
        {
            string decodedHtml = System.Web.HttpUtility.HtmlDecode(html);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(decodedHtml);
            List<InputElement> nodes = new List<InputElement>();

            try
            {
                var inputs = doc.DocumentNode.SelectNodes("//input |//select | //textarea | //img");
                if (inputs != null)
                {
                    foreach (var input in inputs)
                    {
                        var node = new InputElement
                        {
                            TagName = input.Name,
                            Id = input.GetAttributeValue("id", ""),
                            Name = input.GetAttributeValue("name", ""),
                            Value = input.GetAttributeValue("value", ""),
                            Placeholder = input.GetAttributeValue("placeholder", "")
                        };
                        // special handling for img tag
                        if (input.Name == "img" && input.GetAttributeValue("id", "") == "captcha_pic")
                        {
                            System.Drawing.Image img = pbCaptcha.Image;
                            using (var ms = new MemoryStream())
                            {
                                img.Save(ms, img.RawFormat);
                                byte[] imageBytes = ms.ToArray();
                                node.Image = imageBytes;
                            }
                        }

                        if (node.TagName.Length * node.Id.Length > 0)
                            nodes.Add(node);
                    }
                }
            }
            catch (Exception ex) { }
            string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            File.WriteAllText(htmlInputElementJson.FullName, json);
            return json;
        }
        public async Task<string> ParseHtmlAndModifyFieldsAsync(string html, Dictionary<string, string> fieldValues)
        {
            string decodedHtml = System.Web.HttpUtility.HtmlDecode(html);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(decodedHtml);
            List<InputElement> nodes = new List<InputElement>();

            try
            {
                var inputs = doc.DocumentNode.SelectNodes("//input |//select | //textarea | //img");
                if (inputs != null)
                {
                    foreach (var input in inputs)
                    {
                        var node = new InputElement
                        {
                            TagName = input.Name,
                            Id = input.GetAttributeValue("id", ""),
                            Name = input.GetAttributeValue("name", ""),
                            Value = fieldValues.ContainsKey(input.Id) ? fieldValues[input.Id] : fieldValues.ContainsKey(input.Name) ? fieldValues[input.Name] : input.GetAttributeValue("value", ""),
                            Placeholder = input.GetAttributeValue("placeholder", "")
                        };
                        if (input.Name == "img" && input.GetAttributeValue("id", "") == "captcha_pic")
                        {
                        }

                        if (node.TagName.Length * node.Id.Length > 0)
                            nodes.Add(node);
                    }
                }
            }
            catch (Exception ex) { }

            string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return json;
        }
        private string WrapText(string text, int maxWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder wrappedText = new StringBuilder();
            int lineWidth = 0;

            foreach (string word in words)
            {
                int wordWidth = TextRenderer.MeasureText(word, groupBox1.Font).Width;

                if (lineWidth + wordWidth + 10 > maxWidth) // Replace 10 with your desired spacing between words
                {
                    wrappedText.AppendLine();
                    lineWidth = 0;
                }

                wrappedText.Append(word + " ");
                lineWidth += wordWidth + 10;
            }

            return wrappedText.ToString();
        }
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = sender as System.Windows.Forms.RadioButton;
            if (radioButton.Checked)
            {
                //CarInfo selectedCarInfo = radioButton.Tag as CarInfo;
                Dictionary<string, string> fieldValuesDict = new Dictionary<string, string>();

                var radioTextParts = radioButton.Text.Split(',');
                if (radioTextParts.Length >= 3)
                {
                    var licensePlateNumberParts = radioTextParts[0].Split('-');
                    if (licensePlateNumberParts.Length == 2)
                    {
                        fieldValuesDict.Add("LicenseNumber2".ToLower(), licensePlateNumberParts[0].Trim());
                        fieldValuesDict.Add("LicenseNumber3".ToLower(), licensePlateNumberParts[1].Trim());
                    }
                }
                if (radioTextParts.Length >= 4)
                {
                    fieldValuesDict.Add("ViolationDateTime".ToLower(), radioTextParts[3].Trim());
                }
                // Call the SetFieldValuesInWebPageAsync method with the fieldValues dictionary
                SetFieldValuesInWebPageAsync(fieldValuesDict);
            }
        }
        private void btUpload_Click(object sender, EventArgs e)
        {
            string lastPath = Properties.Settings.Default.LastFolderPath;
            if (!string.IsNullOrEmpty(lastPath))
            {
                fbdMediaSelect.SelectedPath = lastPath;
            }
            if (!string.IsNullOrEmpty(lastPath))
            {
                fbdMediaSelect.SelectedPath = lastPath;
            }
            if (fbdMediaSelect.ShowDialog() == DialogResult.OK)
            {
                string path = fbdMediaSelect.SelectedPath;
                Properties.Settings.Default.LastFolderPath = path;
                Properties.Settings.Default.Save();
                var jsonString = ImageHelper.AnalyzeCarNumberAsync(path).Result;
                var carInfoList = JsonConvert.DeserializeObject<List<CarInfo>>(jsonString);
                FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
                flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
                flowLayoutPanel.WrapContents = true;
                flowLayoutPanel.AutoSize = true;
                flowLayoutPanel.AutoScroll = true;
                flowLayoutPanel.Dock = DockStyle.Fill;
                foreach (var carInfo in carInfoList)
                {
                    string text = $"{carInfo.LicensePlateNumber}, {carInfo.CarType}, {carInfo.NormalizedAverageScore.ToString("0.00")}, {carInfo.FirstDisplayTime.ToString("yyyy-MM-dd HH:mm")}";
                    RadioButton radioButton = new RadioButton();
                    radioButton.Text = text;
                    radioButton.Tag = text;
                    radioButton.CheckedChanged += RadioButton_CheckedChanged;
                    radioButton.AutoEllipsis = true;
                    radioButton.Dock = DockStyle.Fill;
                    // Manually wrap the text
                    //radioButton.Text = WrapText(text, groupBox1.Width ); 

                    if (groupBox1.Controls.Count == 0)
                    {
                        radioButton.Checked = true;
                    }
                    groupBox1.Controls.Add(radioButton);
                }

                // Add the FlowLayoutPanel to the GroupBox
                groupBox1.Controls.Add(flowLayoutPanel);


                var fieldValuesDict = AutoFill(new ReportInfo());
                SetFieldValuesInWebPageAsync(fieldValuesDict);
            }
        }
        private void 重整ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pageInfoList.Clear();
            InitializeWebView();
            RefreshWebViewAsync();
        }
        private void ComboBox_DropDown(object sender, EventArgs e)
        {
            if (isDropdownFormOpen)
                return;

            isDropdownFormOpen = true;
            var comboBox = sender as ComboBox;


            // Calculate the maximum width of the items in the ComboBox
            int maxWidth = 0;
            foreach (string item in comboBox.Items)
            {
                int itemWidth = TextRenderer.MeasureText(item, comboBox.Font).Width;
                maxWidth = Math.Max(maxWidth, itemWidth);
            }

            // Create a new form to host the dropdown list
            Form dropdownForm = new Form();
            dropdownForm.FormBorderStyle = FormBorderStyle.FixedDialog; // Set FixedDialog to remove resize borders
            dropdownForm.ControlBox = true; // Hide the maximize and minimize buttons
            dropdownForm.StartPosition = FormStartPosition.CenterScreen; // Set Manual start position to control the form location
            dropdownForm.ShowInTaskbar = false;
            dropdownForm.TopMost = true;

            // Calculate the position to display the dropdown list
            int dropDownTop = comboBox.Top + comboBox.Height;
            int dropDownLeft = Math.Max(comboBox.Left + (comboBox.Width - maxWidth), 0);
            int dropDownWidth = Math.Min(maxWidth, Screen.PrimaryScreen.WorkingArea.Width - dropDownLeft);

            // Set the dropdown form's position and size
            dropdownForm.Left = dropDownLeft;
            dropdownForm.Top = dropDownTop;
            dropdownForm.Width = dropDownWidth;

            // Create a ListBox to host the items
            var listBox = new ListBox();
            listBox.Items.AddRange(comboBox.Items.Cast<object>().ToArray());
            listBox.Font = comboBox.Font;
            listBox.Click += (s, args) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    Dictionary<string, string> fieldValuesDict = new Dictionary<string, string>
                    {
                        { "qclass", listBox.SelectedIndex.ToString() }
                    };
                    // Call the SetFieldValuesInWebPageAsync method with the fieldValues dictionary
                    SetFieldValuesInWebPageAsync(fieldValuesDict);
                    comboBox.SelectedIndex = listBox.SelectedIndex;
                    dropdownForm.Close();
                }
            };
            listBox.BackColor = Color.WhiteSmoke;
            listBox.ForeColor = Color.Black;
            dropdownForm.Height = this.Height / 2;
            // Set the ListBox width to fit the dropdownForm
            listBox.Dock = DockStyle.Fill;
            // Add the ListBox to the dropdown form
            dropdownForm.Controls.Add(listBox);
            dropdownForm.Shown += (s, args) =>
            {
                if (comboBox.SelectedItem != null)
                {
                    // Find the ListBox inside the dropdownForm controls
                    ListBox listBoxInForm = null;
                    foreach (var control in dropdownForm.Controls)
                    {
                        if (control is ListBox lb)
                        {
                            listBoxInForm = lb;
                            break;
                        }
                    }

                    if (listBoxInForm != null)
                    {
                        if (listBoxInForm.Items.Contains(comboBox.SelectedItem))
                        {
                            listBoxInForm.SelectedItem = comboBox.SelectedItem;
                        }
                        else
                            listBox.BackColor = Color.WhiteSmoke;
                    }
                }
            };


            dropdownForm.FormClosed += (s, args) =>
            {
                isDropdownFormOpen = false; // Set the flag to false when the form is closed
            };
            // Show the dropdown form
            if (listBox.Items.Count > 0)
            {
                dropdownForm.Resize += (s, args) =>
                {
                    if (dropdownForm.WindowState == FormWindowState.Minimized || dropdownForm.WindowState == FormWindowState.Maximized)
                    {
                        // Close the form manually
                        isDropdownFormOpen = false;
                        dropdownForm.Close();
                    }
                };


                dropdownForm.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;

            float splitRatio = Properties.Settings.Default.SplitRatio != 0.0f
             ? Properties.Settings.Default.SplitRatio
             : 0.8f;
            splitContainer1.SplitterDistance = (int)(splitContainer1.Width * splitRatio);
        }


        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            float splitRatio = (float)splitContainer1.SplitterDistance / splitContainer1.Width;
            Properties.Settings.Default.SplitRatio = splitRatio;
            Properties.Settings.Default.Save();
        }
        private async Task<string> callOpenAIAsync(List<string> Regulations, string userDescription)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenAIKey"];
            string st = string.Empty;
            OpenAIAPI api = new OpenAIAPI(apiKey);
            var chat = api.Chat.CreateConversation();
            StringBuilder stringBuilder = new StringBuilder();
            // 設定教導指示
            chat.AppendSystemMessage("你是一個專業的律師協助回答法條相關,請根據個案所描述情境找出最合適的法規名或可能符合成立要件的法規名,略過地址之描述只針對違規事項跟道路判定,不須闡述選擇原因,如果沒有請回答:無合適答案");
            chat.AppendSystemMessage("法規表:\n");
            stringBuilder.Append(string.Join(Environment.NewLine, Regulations));
            chat.AppendUserInput("發生在台中市龍井區臺灣大道五段301號\r\n汽車駕駛未禮讓行人先過斑馬線\r\n");
            chat.AppendExampleChatbotOutput("車輛行近行人穿越道或...不暫停讓行人先行通過");
            chat.AppendUserInput("在快速道路上惡意逼車跟不打方向燈任意切換車道");
            chat.AppendExampleChatbotOutput("[快速公路]未依規定行駛切換道路");
            chat.AppendUserInput("在國1之401k處,惡意逼車跟不打方向燈任意切換車道");
            chat.AppendExampleChatbotOutput("任意以迫近、驟然變化車道或...他車讓道");
            chat.AppendSystemMessage(stringBuilder.ToString());
            chat.AppendUserInput($"{userDescription}");
            string response = await chat.GetResponseFromChatbotAsync();
            return response;
        }
        private int SelectRegulations(List<string> Regulations, string Description)
        {
            int index = -1;
            var FindIndex = Regulations.FindIndex(item => item == Description);
            if (FindIndex >= 0)
                return FindIndex;
            var instance1 = new JaroWinkler();

            int bestMatchIndex = -1;
            double bestMatchScore = double.MinValue;

            for (int i = 0; i < Regulations.Count; i++)
            {
                // Calculate the similarity score between the Description and the current item in Regulations
                double similarityScore = instance1.Similarity(Description, Regulations[i]);

                // Update the best match if the current similarity score is higher
                if (similarityScore > bestMatchScore)
                {
                    bestMatchScore = similarityScore;
                    bestMatchIndex = i;
                }
            }

            return bestMatchIndex;
        }

    }
}

