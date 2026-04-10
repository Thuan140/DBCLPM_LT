using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RestaurantAutomation.Pages;
using RestaurantAutomation.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RestaurantAutomation.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;

        // ĐƯỜNG DẪN FILE - Hãy đảm bảo các đường dẫn này tồn tại trên máy bạn
        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "LoginData.json");
        private string excelPath = @"C:\bao dam chat luong phan mem _TH\TestCase_Quản lý quán ăn.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();

            // Tắt các tính năng ghi nhớ mật khẩu của Chrome để tránh hiện popup đè lên giao diện
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("profile.password_manager_leak_detection", false);

            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            loginPage = new LoginPage(driver);
        }

        [Test]
        public void TestLogin_MultiRole_FullFlow()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                string expected = data.ExpectedResult.ToString();

                // LẤY TÊN SHEET TỪ JSON
                string roleSheet = data.SheetName?.ToString() ?? "Test Case (Phục vụ)";

                try
                {
                    driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
                    loginPage.Login(data.Username.ToString(), data.Password.ToString());
                    Thread.Sleep(3000);

                    string currentUrl = driver.Url;
                    string pageSource = loginPage.GetResultText();
                    string validationMsg = loginPage.GetValidationMessage();
                    string actualResult = "";

                    if (currentUrl.Contains(expected) || pageSource.Contains(expected) || validationMsg.Contains(expected))
                    {
                        actualResult = "PASS: Hệ thống phản hồi đúng: " + expected;
                        // Truyền thêm roleSheet vào tham số thứ 5
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actualResult, "Passed", roleSheet, "");
                    }
                    else
                    {
                        actualResult = "FAIL: URL=" + currentUrl + " | Msg=" + validationMsg;
                        string screenshotPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        // Truyền thêm roleSheet vào tham số thứ 5
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actualResult, "Failed", roleSheet, screenshotPath);
                    }

                    if (loginPage.IsLoggedIn())
                    {
                        loginPage.Logout();
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi: " + ex.Message, "Failed", roleSheet, "");
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}