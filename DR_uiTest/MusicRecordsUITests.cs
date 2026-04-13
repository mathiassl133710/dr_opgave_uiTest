using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace DR_uiTest;

// Runs against the web app served on WEB_URL (default: http://localhost:5500)
// Make sure:
//   1. The REST API is running on http://localhost:5268
//   2. The web folder is served, e.g. with VS Code Live Server or:
//        npx serve /path/to/dr_opgave_web -l 5500

public class MusicRecordsUITests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly string _webUrl;

    public MusicRecordsUITests()
    {
        _webUrl = Environment.GetEnvironmentVariable("WEB_URL") ?? "http://localhost:5500";

        var options = new FirefoxOptions();
        options.AddArgument("--headless");   // run without opening a browser window

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    // ------------------------------------------------------------------
    // Helper: wait until the Vue app finishes loading (spinner gone)
    // ------------------------------------------------------------------
    private void WaitForTableToLoad()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        // Wait until #records-table is present in the DOM
        wait.Until(d => d.FindElements(By.Id("records-table")).Count > 0);
    }

    // ------------------------------------------------------------------
    // Test 1: Page title is correct
    // ------------------------------------------------------------------
    [Fact]
    public void PageTitle_ShouldBe_DRMusicRecords()
    {
        _driver.Navigate().GoToUrl(_webUrl);

        Assert.Equal("DR Music Records", _driver.Title);
    }

    // ------------------------------------------------------------------
    // Test 2: The records table is visible after loading
    // ------------------------------------------------------------------
    [Fact]
    public void RecordsTable_ShouldBeVisible_AfterLoad()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var table = _driver.FindElement(By.Id("records-table"));
        Assert.True(table.Displayed);
    }

    // ------------------------------------------------------------------
    // Test 3: Table has the four expected column headers
    // ------------------------------------------------------------------
    [Fact]
    public void RecordsTable_ShouldHave_CorrectHeaders()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var headers = _driver.FindElements(By.CssSelector("#records-table thead th"))
                             .Select(th => th.Text)
                             .ToList();

        Assert.Contains("Title", headers);
        Assert.Contains("Artist", headers);
        Assert.Contains("Duration", headers);
        Assert.Contains("Year", headers);
    }

    // ------------------------------------------------------------------
    // Test 4: At least one record row is rendered
    // ------------------------------------------------------------------
    [Fact]
    public void RecordsTable_ShouldHave_AtLeastOneRow()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var rows = _driver.FindElements(By.CssSelector("#records-table tbody .record-row"));
        Assert.True(rows.Count > 0, "Expected at least one music record row in the table.");
    }

    // ------------------------------------------------------------------
    // Test 5: Known seed record "Bohemian Rhapsody" by Queen is listed
    // ------------------------------------------------------------------
    [Fact]
    public void RecordsTable_ShouldContain_BohemianRhapsody()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var titles = _driver.FindElements(By.CssSelector(".record-title"))
                            .Select(el => el.Text)
                            .ToList();

        Assert.Contains("Bohemian Rhapsody", titles);
    }

    // ------------------------------------------------------------------
    // Test 6: Known seed record has correct artist
    // ------------------------------------------------------------------
    [Fact]
    public void RecordsTable_ShouldContain_QueenAsArtist()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var artists = _driver.FindElements(By.CssSelector(".record-artist"))
                             .Select(el => el.Text)
                             .ToList();

        Assert.Contains("Queen", artists);
    }
}
