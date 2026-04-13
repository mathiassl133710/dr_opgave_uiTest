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
        _webUrl = Environment.GetEnvironmentVariable("WEB_URL") ?? "http://localhost:5500";

        var options = new FirefoxOptions();

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void WaitForTableToLoad()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.Id("records-table")).Count > 0);
    }


    [Fact]
    public void PageTitle_ShouldBe_DRMusicRecords()
    {
        _driver.Navigate().GoToUrl(_webUrl);

        Assert.Equal("DR Music Records", _driver.Title);
    }

    [Fact]
    public void RecordsTable_ShouldBeVisible_AfterLoad()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var table = _driver.FindElement(By.Id("records-table"));
        Assert.True(table.Displayed);
    }

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

    [Fact]
    public void RecordsTable_ShouldHave_AtLeastOneRow()
    {
        _driver.Navigate().GoToUrl(_webUrl);
        WaitForTableToLoad();

        var rows = _driver.FindElements(By.CssSelector("#records-table tbody .record-row"));
        Assert.True(rows.Count > 0, "Expected at least one music record row in the table.");
    }

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
