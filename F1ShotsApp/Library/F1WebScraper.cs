﻿using System.Text.RegularExpressions;
using DatabaseSetupLocal.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace DatabaseSetupLocal.Library;

public static class F1WebScraper
{
    public static List<string> GetRaceResults(int raceYear, int raceNo)
    {
        if (raceNo > 5 && raceYear == 2023)
        {
            raceNo = raceNo - 1;
        }
        var results = new List<string>(20);
        try
        {
            var listOfLinksForRaces = GetUrlsOfRaces(raceYear);
            if (listOfLinksForRaces.Count < raceNo)
            {
                return results;
            }
            var url = listOfLinksForRaces[raceNo - 1];
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            var tableXPath = "//tr";
            var resultsNodes = doc.DocumentNode.SelectNodes(tableXPath).Skip(1)
                .Select(x => x.ChildNodes[7].ChildNodes[5].InnerHtml);
            //doc.DocumentNode.SelectNodes("//tr")[1].ChildNodes[3]
            foreach (var resultsNode in resultsNodes)
            {
                results.Add(resultsNode);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        

        return results;
    }

    public static string GetRaceUrl(int year, int raceNumber)
    {
        var listOfLinksForRaces = GetUrlsOfRaces(year);
        var url = listOfLinksForRaces[raceNumber - 1];
        return url;
    }

    public static string GetPoleSitter(int year, int raceNumber)
    {
        var raceUrl = GetRaceUrl(year, raceNumber);
        var poleSitter = ExtractPoleSitter(raceUrl);

        return poleSitter;
    }

    public static string GetFastestLap(int year, int raceNumber)
    {
        var raceUrl = GetRaceUrl(year, raceNumber);
        var poleSitter = ExtractFastestLap(raceUrl);

        return poleSitter;
    }

    public static string ExtractPoleSitter(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var xPath = "//*[contains(@class, 'side-nav-item')]";
        var temp = doc.DocumentNode.SelectNodes(xPath)[9].InnerHtml;
        var ppUrl = "https://www.formula1.com" + ExtractPpUrl(temp);
        HtmlWeb web2 = new HtmlWeb();

        HtmlDocument doc2 = web2.Load(ppUrl);
        var xPath2 = "//*[contains(@class, 'dark bold')]";
        var temp2 = doc2.DocumentNode.SelectNodes(xPath2);

        return temp2[0].InnerText.Split("\n").Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)).Last();
    }

    public static string ExtractFastestLap(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var xPath = "//*[contains(@class, 'side-nav-item')]";
        var temp = doc.DocumentNode.SelectNodes(xPath)[3].InnerHtml;
        var ppUrl = "https://www.formula1.com" + ExtractPpUrl(temp);
        HtmlDocument doc2 = web.Load(ppUrl);
        var xPath2 = "//*[contains(@class, 'dark bold')]";
        var temp2 = doc2.DocumentNode.SelectNodes(xPath2);

        return temp2[0].InnerText.Split("\n").Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)).Last();
    }

    public static string ExtractPpUrl(string inputString)
    {
        string url = string.Empty;

        int start = inputString.IndexOf("href=\"") + 6; // add 6 to skip "href="""
        int end = inputString.IndexOf("\"", start);

        if (start >= 0 && end >= 0)
        {
            url = inputString.Substring(start, end - start);
        }

        return url;
    }


    public static List<string> GetUrlsOfRaces(int year)
    {
        var url = $"https://www.formula1.com/en/results.html/{year}/races.html";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var xPath = "//*[contains(@class, 'dark bold ArchiveLink')]";
        var listOfOuterHtmls = doc.DocumentNode.SelectNodes(xPath).Select(x => x.OuterHtml).ToList();
        var result = ExtractUrls(listOfOuterHtmls);

        return result;
    }

    public static List<string> ExtractUrls(List<string> outerHtmls)
    {
        var results = new List<string>();

        foreach (var input in outerHtmls)
        {
            // Define a regular expression pattern to match the href attribute
            string pattern = @"href=""([^""]*)""";

            // Use Regex.Match method to find the first occurrence of the pattern in the input string
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Extract the href attribute value from the matched group
                string href = match.Groups[1].Value;
                results.Add("https://www.formula1.com" + href);
            }
            else
            {
                Console.WriteLine("No href attribute found.");
            }
        }

        return results;
    }

    // https://www.formula1.com/en/racing/2023.html
    public static List<string> GetCountryListOfRaces(int year)
    {
        var url = $"https://www.formula1.com/en/racing/{year}.html";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var xPath = "//*[contains(@class, 'event-place')]";
        var result = doc.DocumentNode.SelectNodes(xPath)
            .Select(x => x.ChildNodes.First().InnerHtml.TrimEnd()).ToList();

        return result;
    }

    public static List<string> GetDatesListOfRaces(int year)
    {
        var url = $"https://www.formula1.com/en/racing/{year}.html";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var startXPath = "//*[contains(@class, 'start-date')]";
        var endXPath = "//*[contains(@class, 'start-date')]";
        var monthXPath = "//*[contains(@class, 'month-wrapper f1-wide--xxs')]";
        var resultStart = doc.DocumentNode.SelectNodes(startXPath)
            .Select(x => x.ChildNodes.First().InnerHtml.TrimEnd()).ToList();
        var resultEnd = doc.DocumentNode.SelectNodes(endXPath)
            .Select(x => x.ChildNodes.First().InnerHtml.TrimEnd()).ToList();
        var resultMonth = doc.DocumentNode.SelectNodes(monthXPath)
            .Select(x => x.ChildNodes.First().InnerHtml.TrimEnd()).ToList();
        var result = new List<string>();
        return result;
    }

    public static F1Grid GetDriversData(int year)
    {
        var url = $"https://www.formula1.com/en/results.html/{year}/drivers.html";

        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var fullNameXPath = "//*[contains(@class, 'dark bold')]";
        var numberXPath = "//*[contains(@class, 'dark hide-for-mobile')]";
        var teamXPath = "//*[contains(@class, 'semi-bold uppercase hide-for-tablet')]";
        var resultFullName = doc.DocumentNode.SelectNodes(fullNameXPath).Where((x, i) => i % 2 == 0)
            .Select(x => x.ChildNodes.Where((val, i) => i % 2 != 0).Select(x => x.InnerHtml).Take(2)).ToList();
        var resultFirstName = doc.DocumentNode.SelectNodes(fullNameXPath).Where((x, i) => i % 2 == 0)
            .Select(x => x.ChildNodes.Where((val, i) => i % 2 != 0).Select(x => x.InnerHtml).First()).ToList();
        var resultLastName = doc.DocumentNode.SelectNodes(fullNameXPath).Where((x, i) => i % 2 == 0).Select(x =>
            x.ChildNodes.Where((val, i) => i % 2 != 0).Select(x => x.InnerHtml).Skip(1).First()).ToList();
        var resultAbb = doc.DocumentNode.SelectNodes(fullNameXPath).Where((x, i) => i % 2 == 0).Select(x =>
            x.ChildNodes.Where((val, i) => i % 2 != 0).Select(x => x.InnerHtml).Skip(2).First()).ToList();
        // var resultNumber = doc.DocumentNode.SelectNodes(numberXPath)
        //     .Select(x => x.ChildNodes.Select(x => x.InnerHtml).First()).ToList();
        // var resultTeam = doc.DocumentNode.SelectNodes(teamXPath).Select(x => x.InnerHtml).ToList();
        var f1Grid = new F1Grid();
        f1Grid.Year = year;
        var driversList = new List<Driver>();
        for (int i = 0; i < resultFullName.Count; i++)
        {
            var driver = new Driver();
            // driver.Number = resultNumber[i];
            driver.FirstName = resultFirstName[i];
            driver.LastName = resultLastName[i];
            driver.Abbreviation = resultAbb[i];
            // driver.Team = resultTeam[i];
            driver.FullName = resultFirstName[i] + " " + resultLastName[i];
            driversList.Add(driver);
        }

        f1Grid.Drivers = driversList;

        return f1Grid;
    }

    public static F1Schedule GetScheduleData()
    {
        var url = $"https://f1calendar.com";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var xPath = "//*[contains(@class, 'w-full')]";

        var resultRaces = doc.DocumentNode.SelectNodes(xPath).Skip(2).Select(x =>
                x.ChildNodes.Skip(1)
                    .Select(x => x.ChildNodes.Take(1).Select(x => x.ChildNodes.Skip(1).First().InnerText).First())
                    .ToList())
            .First();

        var resultEvents = doc.DocumentNode.SelectNodes(xPath).Skip(2).Select(x =>
            x.ChildNodes.Skip(1).Select(x =>
                x.ChildNodes.Skip(1).Select(x => x.ChildNodes.Skip(1).First().ChildNodes.Skip(3).First().InnerText)
                    .ToList())).First().ToList();

        var resultDates = doc.DocumentNode.SelectNodes(xPath).Skip(2).Select(x =>
                x.ChildNodes.Skip(1).Select(x => x.ChildNodes.Skip(1).Select(x =>
                        x.ChildNodes.Skip(2).First().InnerHtml + " 2023 " + x.ChildNodes.Skip(3).First().InnerText)
                    .ToList()))
            .First().ToList();

        var resultTimes = doc.DocumentNode.SelectNodes(xPath).Skip(2).Select(x =>
            x.ChildNodes.Skip(1).Select(x =>
                x.ChildNodes.Skip(1).Select(x => x.ChildNodes.Skip(3).First().InnerText).ToList())).First().ToList();

        var f1Schedule = new F1Schedule();
        f1Schedule.Year = 2023;
        var raceScheduleList = new List<RaceSchedule>();

        for (int i = 0; i < resultRaces.Count; i++)
        {
            var raceSchedule = new RaceSchedule();
            var eventList = new List<F1Event>();
            raceSchedule.RaceName = resultRaces[i];

            for (int j = 0; j < 5; j++)
            {
                var f1Event = new F1Event();
                f1Event.EventName = resultEvents[i][j];
                f1Event.EventDateAndTime = Convert.ToDateTime(resultDates[i][j]);
                eventList.Add(f1Event);
            }

            raceSchedule.F1Events = eventList;
            raceScheduleList.Add(raceSchedule);
        }

        f1Schedule.Races = raceScheduleList;
        return f1Schedule;
    }

    public static async Task<List<String>> GetLiveDataAsync()
    {
        var url = $"https://live.planetf1.com/liverace";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        var pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
        var rg = new Regex(pattern);
        var xPath = "/html/body/div[3]/div/main/section/div/div/div[1]/div[1]/div[4]/table/tbody/tr[1]/td[3]/h2/a";
        var resultStart = doc.DocumentNode.SelectNodes("//*[contains(@class, 'signalr_live_race_html')]").First()
            .ChildNodes.Where((value, index) => index % 2 != 0)
            .Select(x => x.ChildNodes[5].ChildNodes[1].ChildNodes[1].InnerText).ToList();

        var res = resultStart.Select(x => rg.Matches(x));
        var result = res.Select(x => x.Select(x => x.Value).ToList().Aggregate((i, j) => i + " " + j)).ToList();


        return result;
    }
}