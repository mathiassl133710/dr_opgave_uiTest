using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace DR_uiTest;

public class MusicRecordsUITests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly string _webUrl;

    public MusicRecordsUITests()
    {
        _webUrl = Environment.GetEnvironmentVariable("WEB_URL") ?? "http://localhost:3000";

        var options = new FirefoxOptions();

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void Login(string username = "admin", string password = "password123!")
    {
        _driver.Navigate().GoToUrl(_webUrl);
        _driver.FindElement(By.Id("input-username")).SendKeys(username);
        _driver.FindElement(By.Id("input-password")).SendKeys(password);
        _driver.FindElement(By.Id("btn-login")).Click();
    }

    private void WaitForTableToLoad()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.Id("records-table")).Count > 0);
    }

    // --- Login tests ---

    [Fact]
    public void PageTitle_ShouldBe_DRMusicRecords()
    {
        _driver.Navigate().GoToUrl(_webUrl);

        Assert.Equal("DR Music Records", _driver.Title);
    }

    [Fact]
    public void LoginForm_ShouldBeVisible_OnStart()
    {
        _driver.Navigate().GoToUrl(_webUrl);

        Assert.True(_driver.FindElement(By.Id("btn-login")).Displayed);
    }

    [Fact]
    public void Login_WithValidCredentials_ShowsRecordsTable()
    {
        Login();
        WaitForTableToLoad();

        Assert.True(_driver.FindElement(By.Id("records-table")).Displayed);
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShowsError()
    {
        Login("admin", "wrongpassword");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.Id("login-error")).Count > 0);

        Assert.True(_driver.FindElement(By.Id("login-error")).Displayed);
    }

    [Fact]
    public void Logout_HidesTable_ShowsLoginForm()
    {
        Login();
        WaitForTableToLoad();

        _driver.FindElement(By.Id("btn-logout")).Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.Id("btn-login")).Count > 0);

        Assert.True(_driver.FindElement(By.Id("btn-login")).Displayed);
    }

    // --- Records table tests ---

    [Fact]
    public void RecordsTable_ShouldHave_CorrectHeaders()
    {
        Login();
        WaitForTableToLoad();

        var headers = _driver.FindElements(By.CssSelector("#records-table thead th"))
                             .Select(th => th.Text)
                             .ToList();

        Assert.Contains("Title", headers);
        Assert.Contains("Artist", headers);
        Assert.Contains("Duration", headers);
        Assert.Contains("Year", headers);
    }

    [Fact]
    public void RecordsTable_ShouldHave_AtLeastOneRow()
    {
        Login();
        WaitForTableToLoad();

        var rows = _driver.FindElements(By.CssSelector("#records-table tbody .record-row"));
        Assert.True(rows.Count > 0);
    }

    [Fact]
    public void RecordsTable_ShouldContain_BohemianRhapsody()
    {
        Login();
        WaitForTableToLoad();

        var titles = _driver.FindElements(By.CssSelector(".record-title"))
                            .Select(el => el.Text)
                            .ToList();

        Assert.Contains("Bohemian Rhapsody", titles);
    }

    [Fact]
    public void RecordsTable_ShouldContain_QueenAsArtist()
    {
        Login();
        WaitForTableToLoad();

        var artists = _driver.FindElements(By.CssSelector(".record-artist"))
                             .Select(el => el.Text)
                             .ToList();

        Assert.Contains("Queen", artists);
    }

    // --- Search tests ---

    [Fact]
    public void SearchByTitle_ShouldFilter_Results()
    {
        Login();
        WaitForTableToLoad();

        _driver.FindElement(By.Id("search-title")).SendKeys("Bohemian");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.CssSelector(".record-title")).Count == 1);

        var titles = _driver.FindElements(By.CssSelector(".record-title"))
                            .Select(el => el.Text)
                            .ToList();

        Assert.Single(titles);
        Assert.Contains("Bohemian Rhapsody", titles);
    }

    [Fact]
    public void SearchByArtist_ShouldFilter_Results()
    {
        Login();
        WaitForTableToLoad();

        _driver.FindElement(By.Id("search-artist")).SendKeys("Queen");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.CssSelector(".record-artist")).Count == 1);

        var artists = _driver.FindElements(By.CssSelector(".record-artist"))
                             .Select(el => el.Text)
                             .ToList();

        Assert.Single(artists);
        Assert.Contains("Queen", artists);
    }

    // --- Add record tests ---

    [Fact]
    public void AddForm_ShouldBeVisible_ForAdmin()
    {
        Login("admin", "password123!");
        WaitForTableToLoad();

        Assert.True(_driver.FindElement(By.Id("btn-add")).Displayed);
    }

    [Fact]
    public void AddForm_ShouldNotBeVisible_ForRegularUser()
    {
        Login("mathias", "mathias123!");
        WaitForTableToLoad();

        Assert.Empty(_driver.FindElements(By.Id("btn-add")));
    }

    [Fact]
    public void AddRecord_ShouldAppear_InTable()
    {
        Login("admin", "password123!");
        WaitForTableToLoad();

        var before = _driver.FindElements(By.CssSelector(".record-row")).Count;

        _driver.FindElement(By.Id("add-title")).SendKeys("Test Song");
        _driver.FindElement(By.Id("add-artist")).SendKeys("Test Artist");
        _driver.FindElement(By.Id("add-duration")).SendKeys("180");
        _driver.FindElement(By.Id("add-year")).SendKeys("2024");
        _driver.FindElement(By.Id("btn-add")).Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.CssSelector(".record-row")).Count > before);

        var titles = _driver.FindElements(By.CssSelector(".record-title"))
                            .Select(el => el.Text)
                            .ToList();

        Assert.Contains("Test Song", titles);
    }

    // --- Delete tests ---

    [Fact]
    public void DeleteButtons_ShouldBeVisible_ForAdmin()
    {
        Login("admin", "password123!");
        WaitForTableToLoad();

        var deleteButtons = _driver.FindElements(By.CssSelector(".btn-delete"));
        Assert.True(deleteButtons.Count > 0);
    }

    [Fact]
    public void DeleteButtons_ShouldNotBeVisible_ForRegularUser()
    {
        Login("mathias", "mathias123!");
        WaitForTableToLoad();

        Assert.Empty(_driver.FindElements(By.CssSelector(".btn-delete")));
    }

    [Fact]
    public void DeleteRecord_ShouldRemoveRow_FromTable()
    {
        Login("admin", "password123!");
        WaitForTableToLoad();

        // Add a temporary record first so we don't delete seed data
        _driver.FindElement(By.Id("add-title")).SendKeys("Temp Delete Song");
        _driver.FindElement(By.Id("add-artist")).SendKeys("Temp Artist");
        _driver.FindElement(By.Id("add-duration")).SendKeys("100");
        _driver.FindElement(By.Id("add-year")).SendKeys("2000");
        _driver.FindElement(By.Id("btn-add")).Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.CssSelector(".record-title"))
            .Any(el => el.Text == "Temp Delete Song"));

        var before = _driver.FindElements(By.CssSelector(".record-row")).Count;

        // Find and click the delete button for the temp record
        var rows = _driver.FindElements(By.CssSelector(".record-row"));
        var tempRow = rows.First(r => r.FindElement(By.CssSelector(".record-title")).Text == "Temp Delete Song");
        tempRow.FindElement(By.CssSelector(".btn-delete")).Click();

        wait.Until(d => d.FindElements(By.CssSelector(".record-row")).Count < before);

        var titles = _driver.FindElements(By.CssSelector(".record-title")).Select(el => el.Text).ToList();
        Assert.DoesNotContain("Temp Delete Song", titles);
    }

    [Fact]
    public void SearchWithNoMatch_ShouldShow_EmptyTable()
    {
        Login();
        WaitForTableToLoad();

        _driver.FindElement(By.Id("search-title")).SendKeys("zzznomatch");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.CssSelector(".record-row")).Count == 0);

        var rows = _driver.FindElements(By.CssSelector(".record-row"));
        Assert.Empty(rows);
    }
}
