# PoliceWebScraping

This project aims to automate the process of reporting traffic violations on the Taichung Traffic Report website. It utilizes web scraping techniques to automatically fill in the required fields, handle CAPTCHA verification, and submit the report. Additionally, ChatGPT, an AI language model, is used to automatically detect violation descriptions based on input text.

## Features

- Automatic filling of required fields on the Taichung Traffic Report website.
- Automated CAPTCHA input for verification.
- Automatic filling of personal information for convenience.
- Utilization of ChatGPT for automated detection of violation descriptions based on provided input.

## Requirements

- Visual Studio with C# support
- .NET Core 3.1 or higher
- AngleSharp NuGet package for HTML parsing
- Newtonsoft.Json NuGet package for JSON serialization
- Windows Forms for the user interface
- Microsoft.Web.WebView2 NuGet package for using WebView2 control

## Installation

1. Clone the repository or download the ZIP file.
2. Open the solution file `PoliceWebScraping.sln` in Visual Studio.
3. Restore NuGet packages if needed.

## How to Use

1. Build and run the application in Visual Studio.
2. Ensure that the `webView21` control loads the Taichung Traffic Report website.
3. Fill in the required personal information fields, if not already pre-filled.
4. Click the "Start" button to initiate the automated process.
5. The program will automatically fill in the required fields, handle CAPTCHA verification, and submit the report.
6. ChatGPT will analyze the provided input text to detect violation descriptions, and the corresponding fields will be filled accordingly.

## Notes

- The program may require adjustments if the website's structure or elements change.
- Use this application responsibly and within the legal framework of the website's terms of service.

## License

This project is licensed under the [MIT License](LICENSE).
