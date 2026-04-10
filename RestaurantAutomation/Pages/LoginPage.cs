using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace RestaurantAutomation.Pages
{
    public class LoginPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- Locators Đăng nhập ---
        private By txtUsername = By.XPath("//input[@type='text' or @type='email']");
        private By txtPassword = By.XPath("//input[@type='password']");
        private By btnLogin = By.CssSelector("button.login-button");

        // --- Locators Đăng xuất (Dropdown) ---
        private By btnUserMenu = By.CssSelector(".btn.btn-link.p-0.text-dark");
        private By btnLogoutItem = By.CssSelector(".dropdown-item.text-danger");

        // --- Hành động Đăng nhập ---
        public void Login(string user, string pass)
        {
            var userElem = wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
            userElem.Clear();
            userElem.SendKeys(user);

            var passElem = driver.FindElement(txtPassword);
            passElem.Clear();
            passElem.SendKeys(pass);

            // Sử dụng JavaScript Click để tránh bị chặn bởi thẻ span bên trong nút
            IWebElement button = wait.Until(ExpectedConditions.ElementToBeClickable(btnLogin));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", button);
        }

        // --- Kiểm tra trạng thái đã đăng nhập chưa ---
        public bool IsLoggedIn()
        {
            try
            {
                // Nếu URL chứa dashboard hoặc tìm thấy nút Menu User thì coi như đã vào trong
                return driver.Url.Contains("dashboard") || driver.FindElements(btnUserMenu).Count > 0;
            }
            catch { return false; }
        }

        // --- Hành động Đăng xuất ---
        public void Logout()
        {
            try
            {
                // Bước 1: Click vào menu để hiện dropdown
                IWebElement userMenu = wait.Until(ExpectedConditions.ElementToBeClickable(btnUserMenu));
                userMenu.Click();

                // Bước 2: Click nút Đăng xuất (dropdown item)
                IWebElement logoutBtn = wait.Until(ExpectedConditions.ElementToBeClickable(btnLogoutItem));
                logoutBtn.Click();

                // Bước 3: Xử lý thông báo "Bạn có chắc chắn muốn đăng xuất?"
                // Đợi Alert xuất hiện trong tối đa 5 giây
                wait.Until(ExpectedConditions.AlertIsPresent());

                // Chuyển hướng điều khiển sang Alert và nhấn OK (Accept)
                IAlert alert = driver.SwitchTo().Alert();
                Console.WriteLine("Thông báo xuất hiện: " + alert.Text);
                alert.Accept();

                // Đợi một chút để hệ thống xóa session và chuyển trang
                Thread.Sleep(1000);
            }
            catch (NoAlertPresentException)
            {
                Console.WriteLine("Không tìm thấy thông báo xác nhận đăng xuất.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi logout: " + ex.Message);
                // Phòng hờ nếu kẹt thì xóa sạch cookie và về trang chủ
                driver.Manage().Cookies.DeleteAllCookies();
                driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
            }
        }
        public string GetValidationMessage()
        {
            try
            {
                // Lấy thông báo lỗi trực tiếp từ thuộc tính validation của HTML5
                string emailMsg = driver.FindElement(txtUsername).GetAttribute("validationMessage");
                string passMsg = driver.FindElement(txtPassword).GetAttribute("validationMessage");

                // Trả về thông báo nào có nội dung (thường là cái đầu tiên bị trống)
                return !string.IsNullOrEmpty(emailMsg) ? emailMsg : passMsg;
            }
            catch { return ""; }
        }
        public string GetResultText()
        {
            try { return driver.PageSource; }
            catch { return ""; }
        }
    }
}