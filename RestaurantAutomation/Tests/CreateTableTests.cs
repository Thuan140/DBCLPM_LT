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
    public class CreateTableTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private TableManagementPage tablePage;

        // ĐƯỜNG DẪN FILE (Bạn điều chỉnh lại cho đúng với máy của mình)
        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "TableData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\LT\Testcase.xlsx";

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
            tablePage = new TableManagementPage(driver);

            // BƯỚC ĐỆM: Đăng nhập trước khi kiểm tra chức năng bàn
            driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
            loginPage.Login("ngochoa@gmail.com", "123456");

            // Đợi chuyển hướng đến Dashboard
            Thread.Sleep(3000);
        }

        [Test]
        public void Test_CreateTable_MultiCases()
        {
            // 1. Đọc dữ liệu từ file JSON
            string jsonContent = File.ReadAllText(jsonPath);
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            foreach (var data in testDataList)
            {
                string tcID = data.TestCaseID.ToString();
                string expected = data.ExpectedResult.ToString().ToLower();
                string roleSheet = data.SheetName?.ToString() ?? "Test Case (Phục vụ)";

                try
                {
                    // 2. Refresh trang hoặc chuyển hướng về trang quản lý bàn để đảm bảo trạng thái sạch
                    driver.Navigate().Refresh();
                    Thread.Sleep(2000);

                    // 3. Mở Form tạo bàn
                    tablePage.OpenCreateForm();
                    Thread.Sleep(1000);

                    // 4. Điền thông tin từ file JSON
                    tablePage.FillCreateTableForm(
                        data.TableNumber?.ToString(),
                        data.Capacity?.ToString(),
                        data.Location?.ToString(),
                        data.Notes?.ToString()
                    );

                    // 5. Nhấn nút Tạo bàn
                    tablePage.SubmitForm();
                    Thread.Sleep(2000); // Đợi hệ thống xử lý và hiển thị thông báo

                    // 6. Kiểm tra kết quả
                    string pageSource = driver.PageSource.ToLower();
                    string validationMsg = tablePage.GetValidationMessage().ToLower();
                    string actualResult = "";

                    // Kiểm tra nếu thông báo xuất hiện trong Page Source hoặc Validation của HTML5
                    if (pageSource.Contains(expected) || validationMsg.Contains(expected))
                    {
                        actualResult = "PASS: Hệ thống phản hồi đúng mong đợi: " + expected;
                        ExcelHelper.UpdateExcel(excelPath, tcID, actualResult, "Passed", roleSheet, "");
                    }
                    else
                    {
                        actualResult = "FAIL: Không tìm thấy thông báo: " + expected;
                        string screenshotPath = CaptureHelper.TakeScreenshot(driver, tcID);
                        ExcelHelper.UpdateExcel(excelPath, tcID, actualResult, "Failed", roleSheet, screenshotPath);
                    }
                }
                catch (Exception ex)
                {
                    ExcelHelper.UpdateExcel(excelPath, tcID, "Lỗi ngoại lệ: " + ex.Message, "Failed", roleSheet, "");
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