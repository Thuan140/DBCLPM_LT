using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace RestaurantAutomation.Pages
{
    public class OrderPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public OrderPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Locator động để tìm món ăn theo tên
        private By MenuItem(string itemName) => By.XPath($"//h6[contains(@class, 'menu-item-name') and text()='{itemName}']");

        private By txtOrderNotes = By.Id("orderNotes");
        private By btnSubmitOrder = By.Id("submitOrder");
        private By btnCancelOrder = By.CssSelector(".btn-cancel");

        public void SelectFood(string foodName)
        {
            IWebElement item = wait.Until(ExpectedConditions.ElementToBeClickable(MenuItem(foodName)));
            // Sử dụng JS Click để tránh lỗi nếu card món ăn bị lồng nhiều lớp div
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", item);
        }

        public void EnterNotes(string notes)
        {
            IWebElement notesElem = wait.Until(ExpectedConditions.ElementIsVisible(txtOrderNotes));
            notesElem.Clear();
            notesElem.SendKeys(notes ?? "");
        }

        public void ClickCreateOrder()
        {
            IWebElement btn = wait.Until(ExpectedConditions.ElementToBeClickable(btnSubmitOrder));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
        }
        public string GetSuccessMessage(string expectedText)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                // Script này sẽ tìm tất cả các span và chỉ lấy cái nào chứa expectedText
                string script = $@"
            var spans = document.querySelectorAll('div.alert-info div.d-flex span');
            for (var i = 0; i < spans.length; i++) {{
                if (spans[i].innerText.toLowerCase().includes('{expectedText.ToLower()}')) {{
                    return spans[i].innerText;
                }}
            
            return '';";

                // Rình trong 6 giây vì Vercel có thể phản hồi chậm sau khi bấm nút
                for (int i = 0; i < 12; i++)
                {
                    string result = js.ExecuteScript(script)?.ToString();
                    if (!string.IsNullOrEmpty(result)) return result.Trim();
                    Thread.Sleep(500);
                }
            }
            catch { }
            return "";
        }
        public bool IsSubmitButtonEnabled()
        {
            try
            {
                IWebElement btn = driver.FindElement(btnSubmitOrder);
                // Kiểm tra đồng thời cả class 'disabled' và thuộc tính 'disabled' của HTML
                bool hasDisabledClass = btn.GetAttribute("class").Contains("disabled");
                bool hasDisabledAttr = btn.GetAttribute("disabled") != null;
                return !hasDisabledClass && !hasDisabledAttr;
            }
            catch { return false; }
        }
        public string GetSuccessTitleText()
        {
            try
            {
                // Chờ modal success hiện lên và lấy text từ id successTitle
                IWebElement title = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("successTitle")));
                return title.Text.Trim();
            }
            catch { return ""; }
        }

        // Hàm xử lý Browser Alert (Cho nút Force Clean)
        public void AcceptBrowserAlert()
        {
            try
            {
                wait.Until(ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert().Accept();
            }
            catch { }
        }
        public void ClickCancel()
        {
            driver.FindElement(btnCancelOrder).Click();
        }
    }
}