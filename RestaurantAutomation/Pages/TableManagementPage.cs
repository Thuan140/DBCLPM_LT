using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace RestaurantAutomation.Pages
{
    public class TableManagementPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public TableManagementPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }
        

        // --- Locators trong Form Tạo bàn ---
        private By txtTableNumber = By.Id("tableNumber");
        private By drpCapacity = By.Id("tableCapacity");
        private By drpLocation = By.Id("tableLocation");
        private By txtNotes = By.Id("tableNotes");
        // Nút mở Form (có icon plus, nằm ngoài danh sách)
        private By btnOpenCreateForm = By.XPath("//button[contains(@onclick, 'showCreateTableModal')]");

        // Nút Xác nhận tạo (nằm trong Footer của Modal/Form)
        private By btnSubmitCreate = By.XPath("//div[contains(@class, 'modal-footer')]//button[contains(., 'Tạo bàn')]");

        // Nút Hủy (nằm trong Footer của Modal/Form)
        private By btnCancel = By.XPath("//div[contains(@class, 'modal-footer')]//button[contains(., 'Hủy')]");

        // --- Hành động ---
        public void OpenCreateForm()
        {
            // Chờ nút mở form xuất hiện và click
            IWebElement btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnOpenCreateForm));
            btn.Click();
        }

        public void FillCreateTableForm(string number, string capacity, string location, string notes)
        {
            // Nhập số bàn
            var inputNumber = wait.Until(ExpectedConditions.ElementIsVisible(txtTableNumber));
            inputNumber.Clear();
            if (!string.IsNullOrEmpty(number)) inputNumber.SendKeys(number);

            // Chọn số chỗ ngồi (Dùng SelectElement của Selenium)
            if (!string.IsNullOrEmpty(capacity))
            {
                var selectCap = new SelectElement(driver.FindElement(drpCapacity));
                selectCap.SelectByValue(capacity); // Truyền giá trị "2", "4", "6"...
            }

            // Chọn vị trí
            if (!string.IsNullOrEmpty(location))
            {
                var selectLoc = new SelectElement(driver.FindElement(drpLocation));
                selectLoc.SelectByValue(location); // Truyền giá trị "indoor", "outdoor", "vip"...
            }

            // Nhập ghi chú
            driver.FindElement(txtNotes).Clear();
            driver.FindElement(txtNotes).SendKeys(notes);
        }

        public void SubmitForm()
        {
            try
            {
                // Đợi nút sẵn sàng
                IWebElement button = wait.Until(ExpectedConditions.ElementToBeClickable(btnSubmitCreate));

                // Sử dụng JavaScript Click để tránh bị icon <svg> bên trong chặn
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", button);

                Console.WriteLine("Đã click nút Tạo bàn bằng JS.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Không thể click nút Tạo bàn: " + ex.Message);
                // Nếu JS fail thì thử click bình thường như phương án dự phòng
                driver.FindElement(btnSubmitCreate).Click();
            }
        }

        // Lấy thông báo lỗi từ Validation HTML5 hoặc thông báo Toast (nếu có)
        public string GetValidationMessage()
        {
            // Thử lấy tin nhắn từ trường Số bàn nếu nó trống/lỗi
            return driver.FindElement(txtTableNumber).GetAttribute("validationMessage");
        }
    }
}