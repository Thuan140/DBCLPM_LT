using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

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
        // Locator cho các nút trong Modal Thao tác bàn
        private By btnFinishTable = By.XPath("//button[contains(., 'Dọn bàn xong')]");
        private By btnAddMoreFood = By.XPath("//button[contains(., 'Thêm món')]");
        private By btnForceClean = By.XPath("//button[contains(., 'Force Clean')]");
        private By btnCallStaff = By.XPath("//button[contains(., 'Gọi phục vụ')]");

        // Các hàm thao tác
        public void ClickFinishTable() => wait.Until(ExpectedConditions.ElementToBeClickable(btnFinishTable)).Click();
        public void ClickAddMoreFood() => wait.Until(ExpectedConditions.ElementToBeClickable(btnAddMoreFood)).Click();
        public void ClickForceClean() => wait.Until(ExpectedConditions.ElementToBeClickable(btnForceClean)).Click();
        public void ClickCallStaff() => wait.Until(ExpectedConditions.ElementToBeClickable(btnCallStaff)).Click();
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

        public void ClickTableByNumber(string tableNumber)
        {
            // Tìm thẻ div có class table-number mà chứa số bàn từ JSON
            // XPath này sẽ tìm bàn dựa trên phần số (ví dụ: -0586598) nằm trong chữ "Bàn -0586598"
            string xpath = $"//div[contains(@class, 'table-number') and contains(text(), '{tableNumber}')]";

            try
            {
                // Đợi bàn xuất hiện và cuộn tới nó
                IWebElement tableNumElem = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                // Leo lên thẻ cha (table-card) để click
                IWebElement tableCard = tableNumElem.FindElement(By.XPath("./.."));

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", tableCard);
                Thread.Sleep(500);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", tableCard);
            }
            catch (Exception ex)
            {
                throw new Exception($"Không tìm thấy bàn có số: {tableNumber}. Lỗi: {ex.Message}");
            }
        }
        public void WaitForDataToRender()
        {
            // Đợi tối đa 30 giây
            WebDriverWait longWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            try
            {
                // Điều kiện: Đợi cho đến khi thẻ div#tablesGrid chứa ít nhất 1 bàn (table-card)
                // Và bàn đó phải có text (không bị rỗng số bàn)
                longWait.Until(d => {
                    var tables = d.FindElements(By.ClassName("table-card"));
                    if (tables.Count > 0)
                    {
                        // Kiểm tra xem bàn đầu tiên đã hiện số bàn chưa (tránh trường hợp hiện khung nhưng rỗng chữ)
                        string tableText = tables[0].Text.Trim();
                        return !string.IsNullOrEmpty(tableText);
                    }
                    return false;
                });

                Console.WriteLine("---> Dữ liệu đã lên đầy đủ!");
            }
            catch (WebDriverTimeoutException)
            {
                // Nếu đơ quá lâu, thay vì Refresh, hãy thử click vào nút "Quản lý bàn" 
                // ở sidebar để kích hoạt lại hàm load dữ liệu của web
                Console.WriteLine("---> Dữ liệu vẫn trống, thử kích hoạt lại sidebar...");
                IWebElement menuTable = driver.FindElement(By.XPath("//span[contains(text(), 'Quản lý bàn')]"));
                menuTable.Click();
                Thread.Sleep(3000);
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