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
    public class CasherTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private CasherPage casherPage;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "CasherData.json");
        private string excelPath = @"C:\Users\HP\Downloads\TestCase_Quản lý quán ăn.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();

            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("profile.password_manager_leak_detection", false);

            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            loginPage = new LoginPage(driver);
            casherPage = new CasherPage(driver);
        }

        [Test]
        public void TestCasher_Flow_RealUser()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                string action = data.Action.ToString();
                string expected = data.ExpectedResult.ToString();
                string roleSheet = data.SheetName.ToString();

                try
                {
                    // ===== LOGIN =====
                    driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
                    loginPage.Login(data.Username.ToString(), data.Password.ToString());
                    Thread.Sleep(8000);

                    if (!loginPage.IsLoggedIn())
                        throw new Exception("Login fail");

                    // ===== ACTION =====
                    switch (action)
                    {
                        case "ViewPaymentList":
                            casherPage.GoToPayment();
                            break;

                        case "ProcessPayment":
                            casherPage.GoToPayment();
                            casherPage.ProcessPayment();
                            break;

                        case "ViewHistory":
                            casherPage.GoToHistory();
                            break;

                        case "ViewHistoryDetail":
                            casherPage.GoToHistory();
                            casherPage.ViewHistoryDetail();
                            break;

                        case "ViewDashboard":
                            casherPage.GoToDashboard();
                            break;
                    }

                    Thread.Sleep(2000);

                    // ===== VERIFY =====
                    if (driver.PageSource.Contains(expected))
                    {
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "PASS", "Passed", roleSheet, "");
                    }
                    else
                    {
                        string img = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "FAIL", "Failed", roleSheet, img);
                    }

                    loginPage.Logout();
                }
                catch (Exception ex)
                {
                    string img = CaptureHelper.TakeScreenshot(driver, testCaseID);
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, ex.Message, "Failed", roleSheet, img);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
