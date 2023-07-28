using FFMediaToolkit.Graphics;
using Google.Cloud.Vision.V1;
using Imgur.API.Authentication;
using Imgur.API.Endpoints;
using Imgur.API.Models;
using kezLPR;
using Newtonsoft.Json;
using OpenAI_API.Images;
using RestSharp;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static PoliceWebScraping.AzureResponse;
using static PoliceWebScraping.ImageHelper;
using Image = Google.Cloud.Vision.V1.Image;

namespace PoliceWebScraping
{
    public class ImgBbApiClient
    {
        private readonly RestClient restClient;
        private readonly string apiKey;

        public ImgBbApiClient(string apiKey)
        {
            this.apiKey = apiKey;
            restClient = new RestClient("https://api.imgbb.com/1/");
        }

        public string UploadImage(Bitmap image)
        {
            try
            {
                byte[] imageData;
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png); // Save the image to a memory stream as PNG
                    imageData = ms.ToArray();
                }

                var request = new RestRequest("upload", Method.Post);
                request.AddParameter("key", apiKey);
                request.AddFile("image", imageData, "image.png");

                var response = restClient.Execute<ImgBbResponse>(request);
                if (response.IsSuccessful && response.Data != null)
                {
                    return response.Data.data.url;
                }
                else
                {
                    Console.WriteLine($"Error uploading image. StatusCode: {response.StatusCode}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }
    }
    static public class ImageHelper
    {
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);

        public class AzureAccount
        {
            public string OCRKey { get; set; }
            public string VisionEndpoint { get; set; }
        }
        public class ImgurAccount
        {
            public string ID { get; set; }
            public string Secret { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public string AccountID { get; set; }
            public string ExpiresIn { get; set; }
        }
        public class CarInfo
        {
            public string LicensePlateNumber { get; set; }
            public int TotalScore { get; set; }
            public double AverageScore { get; set; }
            public string CarType { get; set; }
            public double NormalizedAverageScore { get; set; }
            public DateTime FirstDisplayTime { get; set; } = new DateTime();
        }
        public class ImgurResponse
        {
            public ImgurData data { get; set; }
            public bool success { get; set; }
            public int status { get; set; }
        }

        public class ImgurData
        {
            public string link { get; set; }
        }
        public class ImgBbResponse
        {
            public ImgBbData data { get; set; }
        }

        public class ImgBbData
        {
            public string url { get; set; }
        }
        static public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov" };
        public static string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public delegate void UpdateTextDelegate(System.Windows.Forms.TextBox textBox, string text);
        static public UpdateTextDelegate UpdateText = (textBox, text) =>
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => textBox.Text = text));
            }
            else
            {
                textBox.Text = text;
            }
        };
        public delegate void UpdateComboBoxDelegate(ComboBox comboBox, List<string> options);
        static public void UpdateComboBox(ComboBox comboBox, List<string> options)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(new UpdateComboBoxDelegate(UpdateComboBox), comboBox, options);
            }
            else
            {
                comboBox.DataSource = options;
            }
        }
        private delegate void SelectComboBoxItemDelegate(ComboBox comboBox, int index);
        public static void SelectComboBoxItem(ComboBox comboBox, int index)
        {
            if (index >= 0 && index < comboBox.Items.Count)
            {
                comboBox.Invoke(new SelectComboBoxItemDelegate((cb, idx) =>
                {
                    cb.SelectedIndex = idx;
                }), comboBox, index);
            }
        }

        public static async Task<string> AnalyzeCarNumberAsync(string directoryPath, int top = 5)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            string newImageFolderPath = Path.Combine(directoryInfo.FullName, "NewImage");
            if (Directory.Exists(newImageFolderPath))
            {
                Directory.Delete(newImageFolderPath, true);
            }
            string[] videoFiles = Directory.GetFiles(directoryInfo.FullName)
                .Where(file => videoExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .ToArray();
            foreach (var videoFile in videoFiles)
            {
                VideoScreenshot.CaptureScreenshots(videoFile);
            }

            string[] imageFiles = Directory.GetFiles(directoryInfo.FullName)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (!Directory.Exists(newImageFolderPath))
            {
                Directory.CreateDirectory(newImageFolderPath);
            }
            var carInfoDict = new Dictionary<string, List<(double similarity, string carType, DateTime recordDatetime)>>();

            foreach (var imageFile in imageFiles)
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


                string newImagePath = Path.Combine(newImageFolderPath, Path.GetFileName(imageFile));
                File.Copy(imageFile, newImagePath, true);
            }

            imageFiles = Directory.GetFiles(newImageFolderPath)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .ToArray();


            Parallel.ForEach(imageFiles, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 }, imageFile =>
            {
                var cts = new CancellationTokenSource();
                var callApi = false;
                var task = Task.Run(async () =>
                {
                    using (var carImage = new Bitmap(imageFile))
                    {
                        var kLPR = new kLPR
                        {
                            minHeight = 10,
                            maxHeight = 150
                        };
                        var carNu = kLPR.DoLPRZ(carImage);
                        var carInfoArray = carNu.Split(',');
                        if (carInfoArray.Length >= 3)
                        {
                            string licensePlateNumber = carInfoArray[0];
                            string normalizedNumber = NormalizeLicensePlateNumber(licensePlateNumber);
                            if (!Regex.IsMatch(licensePlateNumber, "^[a-zA-Z0-9-]+$"))
                                return;
                            double similarity = double.Parse(carInfoArray[1]);
                            string carTypeValue = carInfoArray[^2];
                            DateTime dateTime = new DateTime();
                            lock (carInfoDict) // 加上鎖定，避免多執行緒同時存取字典
                            {
                                if (carInfoDict.ContainsKey(licensePlateNumber))
                                {
                                    carInfoDict[licensePlateNumber].Add((similarity, carTypeValue, dateTime));
                                }
                                else
                                {
                                    carInfoDict[licensePlateNumber] = new List<(double similarity, string carType, DateTime datetime)>
                                        {
                                            (similarity, carTypeValue, dateTime)
                                        };
                                    callApi = true;
                                }
                            }
                            if (callApi)
                            {
                                await semaphore.WaitAsync();
                                try
                                {
                                    var displayDatetimeSt = await AnalyzeImageWithGoogleApi(CropImage(carImage));
                                    DateTime displayDatetime;

                                    // Try to convert the displayDatetimeSt to a DateTime
                                    if (DateTime.TryParseExact(displayDatetimeSt, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out displayDatetime))
                                    {
                                        List<(double similarity, string carType, DateTime recordDatetime)> updatedCarInfoList = new List<(double similarity, string carType, DateTime recordDatetime)>();
                                        lock (carInfoDict)
                                        {
                                            // Create a temporary list with updated recordDatetime values
                                            if (carInfoDict.ContainsKey(licensePlateNumber))
                                            {
                                                foreach (var carInfo in carInfoDict[licensePlateNumber])
                                                {
                                                    updatedCarInfoList.Add((carInfo.similarity, carInfo.carType, displayDatetime));
                                                }
                                            }
                                        }

                                        lock (carInfoDict)
                                        {
                                            if (carInfoDict.ContainsKey(licensePlateNumber))
                                            {
                                                carInfoDict[licensePlateNumber] = updatedCarInfoList;
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                        }
                    }
                });

                var timeout = TimeSpan.FromSeconds(5);
                if (!task.Wait((int)timeout.TotalMilliseconds, cts.Token))
                {
                    // 若超過逾時時間，取消執行任務
                    cts.Cancel();
                    Debug.WriteLine($"Processing of file {imageFile} timed out.");
                }
            });



            List<CarInfo> topCarInfoList = carInfoDict.OrderByDescending(kv => kv.Value.Count)
                                           .Take(top)
                                           .Select(kv => new CarInfo
                                           {
                                               LicensePlateNumber = kv.Key,
                                               TotalScore = kv.Value.Sum(info => (int)(info.similarity * 100)),
                                               AverageScore = CalculateWeightedAverage(kv.Value, info => info.similarity),
                                               CarType = kv.Value.FirstOrDefault().carType,
                                               FirstDisplayTime = kv.Value.FirstOrDefault().recordDatetime,
                                           })
                                           .ToList();

            string topCarInfoListresult = JsonConvert.SerializeObject(topCarInfoList);

            double maxScore = topCarInfoList.Max(item => item.AverageScore);
            double minScore = topCarInfoList.Min(item => item.AverageScore);

            var normalizedCarInfoList = topCarInfoList.Select(item => new CarInfo
            {
                LicensePlateNumber = item.LicensePlateNumber,
                TotalScore = item.TotalScore,
                NormalizedAverageScore = NormalizeScore(item.AverageScore, maxScore, minScore),
                CarType = item.CarType,
               FirstDisplayTime = item.FirstDisplayTime,
            })
             .OrderByDescending(item => item.NormalizedAverageScore)
             .Where(item => item.NormalizedAverageScore >= 50)
             .ToList();

            string result = JsonConvert.SerializeObject(normalizedCarInfoList);

            return result;
        }
        public static async Task<string> CallComputerVisionApi(AzureAccount azureAccount, ImgurAccount imgurAccount, System.Drawing.Image image)
        {
            try
            {
                string imageUrl = string.Empty, imageDeleteHash = string.Empty;
                ApiClient apiClient = null;
                HttpClient httpClient = new HttpClient();
                try
                {
                    apiClient = new ApiClient(imgurAccount.ID, imgurAccount.Secret);
                    var oAuth2Endpoint = new OAuth2Endpoint(apiClient, httpClient);
                    var authUrl = oAuth2Endpoint.GetAuthorizationUrl();
                    var token = new OAuth2Token
                    {
                        AccessToken = imgurAccount.AccessToken,
                        RefreshToken = imgurAccount.RefreshToken,
                        AccountId = int.Parse(imgurAccount.AccountID),
                        AccountUsername = imgurAccount.ID,
                        ExpiresIn = int.Parse(imgurAccount.ExpiresIn),
                        TokenType = "bearer"
                    };
                    apiClient.SetOAuth2Token(token);

                    var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
                    (imageUrl, imageDeleteHash) = await UploadImageToImgurAsync((System.Drawing.Image)image.Clone(), imageEndpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during image upload: {ex.Message}");
                    return string.Empty;
                }
                finally
                {
                    httpClient?.Dispose();
                }

                string contentText = await AnalyzeImageWithComputerVisionApi(azureAccount.VisionEndpoint, azureAccount.OCRKey, imageUrl);

                try
                {
                    httpClient = new HttpClient();
                    var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
                    await imageEndpoint.DeleteImageAsync(imageDeleteHash);
                }
                finally
                {
                    httpClient?.Dispose();
                }

                return contentText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }
        private static async Task<string> AnalyzeImageWithGoogleApi(System.Drawing.Image image)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", new FileInfo("./dll/googleAPI.json").FullName);
            string Contant = string.Empty;
            using (var bp = new Bitmap(image))
            {
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    bp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = ms.ToArray();
                }

                // Perform OCR on the image using the Google Cloud Vision API
                try
                {
                    var client = ImageAnnotatorClient.Create();

                    var imageFromBytes = Image.FromBytes(imageBytes);

                    // Detect text in the image
                    IReadOnlyList<EntityAnnotation> textAnnotations = await client.DetectTextAsync(imageFromBytes);
                    Contant = textAnnotations.Select(s => s.Description).OrderByDescending(s => s.Length).FirstOrDefault()?.ToString();
                }
                catch (Exception ex)
                {
                }
                return Contant;

            }
        }
        private static async Task<string> AnalyzeImageWithComputerVisionApi(string VISION_ENDPOINT, string AzureOCRKey, string imageUrl, char split = '_')
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AzureOCRKey);

                string apiUrl = $"{VISION_ENDPOINT}/computervision/imageanalysis:analyze?features=caption,read&model-version=latest&language=en&api-version=2023-02-01-preview";

                var requestBody = new { url = imageUrl };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ResponseJson>(responseJson);
                    // Concatenate the content of the words with the specified separator
                    string resultContent = string.Join(split, responseObject.readResult.pages[0].words.Select(w => w.content));
                    return resultContent;
                }
                else
                {
                    Console.WriteLine($"Error occurred. StatusCode: {response.StatusCode}");
                    return string.Empty;
                }
            }
        }
        private static async Task<(string, string)> UploadImageToImgurAsync(System.Drawing.Image img, ImageEndpoint imageEndpoint)
        {
            string ImgURL = string.Empty;
            string ImgDeleteHash = string.Empty;
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);  // Save as PNG format
                ms.Position = 0;  // Reset stream position

                var imageUpload = await imageEndpoint.UploadImageAsync(ms);
                if (imageUpload != null && imageUpload.Link != null)
                {
                    ImgURL = imageUpload.Link;
                    ImgDeleteHash = imageUpload.DeleteHash;
                }

            }
            return (ImgURL, ImgDeleteHash);
        }
        private static string NormalizeLicensePlateNumber(string licensePlateNumber)
        {
            string normalizedNumber = Regex.Replace(licensePlateNumber, "[^a-zA-Z0-9-]", "");
            return normalizedNumber;
        }
        private static double CalculateWeightedAverage<T>(IEnumerable<T> items, Func<T, double> valueSelector)
        {
            double sum = 0;
            int count = 0;
            foreach (var item in items)
            {
                double value = valueSelector(item);
                sum += value;
                count++;
            }
            if (count > 0)
            {
                double weightedSum = sum * Math.Sqrt(count); // 使用平方根作為個數的加權因子
                double weightedAverage = weightedSum / count;
                return weightedAverage;
            }
            return 0;
        }
        private static double NormalizeScore(double score, double maxScore, double minScore)
        {
            double normalizedScore = (score - minScore) / (maxScore - minScore) * 100;
            return Math.Round(normalizedScore, 2);
        }
        public static Bitmap CropImage(Bitmap originalImage, float heightRatio = 0.3f, float widthRatio = 0.3f)
        {
            heightRatio = Math.Min(1.0f, Math.Max(0.0f, heightRatio));
            widthRatio = Math.Min(1.0f, Math.Max(0.0f, widthRatio));

            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;

            int cropWidth = (int)(originalWidth * widthRatio);
            int cropHeight = (int)(originalHeight * heightRatio);

            Rectangle cropRectangle = new Rectangle(originalWidth - cropWidth, originalHeight - cropHeight, cropWidth, cropHeight);
            Bitmap croppedImage = originalImage.Clone(cropRectangle, originalImage.PixelFormat);

            return croppedImage;
        }
    }
}
