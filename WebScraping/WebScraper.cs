using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;

using System.Net.Http;
using System.Threading;
using System.Windows;
using System.IO;
using System.Globalization;

namespace WebScraping
{

    public class CityDetail
    {
        public CityDetail()
        {
            UniversalDetail = new List<string>();
        }
        public List<string> UniversalDetail { get; set; }

        public void PrintInConsole()
        {
            Console.WriteLine("start from here -----------");
            foreach(var a in UniversalDetail)
            {
                Console.WriteLine(a);
            }
            Console.WriteLine("ended here -----------");
        }

        public string PrintInString(string delimiter)
        {
            string res = null;
            foreach(var a in UniversalDetail)
            {
                res += a + delimiter;
            }
            return res;
        }
    }

    public partial class WebScraper : Form
    {
        public WebScraper()
        {
            InitializeComponent();
        }

        private string theUrl = "https://www.weather.go.kr/w/wnuri-fct/weather/sfc-city-weather.do?stn=&type=t99&reg=100&tm=&unit=km%2Fh";
        private string currentDateTime;

        private void start_btn_Click(object sender, EventArgs e)
        {
            StartScraping();
        }

        internal async void StartScraping()
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(theUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);

            //Add connection between initial scrape, and parsing of results
            GetScrapeResults(document);
        }

        private static string[] GetFileNames(string path, string filter, int limit)
        {
            string[] files = Directory.GetFiles(path, filter);

            if(files.Length < limit)
            {
                for (int i = 0; i < files.Length; i++)
                    files[i] = Path.GetFileName(files[i]);
                return files;
            }
            else
            {
                for (int i = 0; i < limit; i++)
                    files[i] = Path.GetFileName(files[i]);
                return files;
            }
        }

        private int CheckLocalFileName(string[] name, string nowDate)
        {
            foreach(var n in name)
            {
                if(n == nowDate + ".00.txt")
                {
                    return 1;
                }
            }
            return 0;
        }

        private int NeedToDownload(IHtmlDocument document)
        {
            //init current server time
            InitServerDateTime(document);

            //current time
            //DateTime local = DateTime.Now;
            //string localDateStr = local.ToString("yyyy-MM-dd-HH");
            //Console.WriteLine("local time: " + localDateStr);

            //check local files
            //aaaaa, just do that later :"
            string thePath = Directory.GetCurrentDirectory() + "\\output\\";
            string[] local_files = GetFileNames(thePath, "*.txt", 3);

            int checkRes = CheckLocalFileName(local_files, currentDateTime);

            if(checkRes == 1)
            {
                //it exist in local file
                return 0;
            }
            else
            {
                //need to download
                return 1;
            }

            ////website time
            //string webDateStr;
            //IEnumerable<IElement> articleLink = null;
            //articleLink = document.All.Where(x => x.ClassName == "ann-txt");
            ////cleaning
            //foreach (var a in articleLink) //just once but idk why have to be foreach
            //{
            //    webDateStr = CleanAndGetDateTime(a);
            //    return 1;
            //    //Console.WriteLine("ann-txt get: " + a.InnerHtml);
            //}

            ////no need to download
            //return -1;
            
        }

        private void InitServerDateTime(IHtmlDocument document)
        {
            IEnumerable<IElement> articleLink = null;
            articleLink = document.All.Where(x => x.ClassName == "ann-txt");

            string htmlResult = null;
            foreach (var art in articleLink)
            {
                htmlResult = art.InnerHtml;
                //Console.WriteLine(art.InnerHtml);
            }

            htmlResult = htmlResult.ReplaceFirst("<strong>기상실황표</strong>", "");

            htmlResult = htmlResult.Remove(htmlResult.Length-3, 3);
            htmlResult = htmlResult.Replace(".", "-");
            Console.WriteLine("time result: " + htmlResult);

            currentDateTime = htmlResult;
            //int a = 10;
        }

        public void PrintResults(IEnumerable<IElement> articleLink)
        {

            List<CityDetail> temporary_data = new List<CityDetail>();

            foreach (var element in articleLink)
            {
                //if (element.InnerHtml.Contains("tbody"))
                //{
                //    for(var i = 0, row; row = element.ChildElementCount())
                //}
                //Console.WriteLine("scrape result:" + element.InnerHtml);
                int itt = 0;

                List<string> header_str = new List<string>();

                //Console.WriteLine("scrape result:" + element.FirstElementChild);
                foreach (var b in element.Children)
                {
                    Console.WriteLine("itterate: " + itt);
                    //table header
                    if (itt == 2)
                    {
                        //only run once, idk why. oh beacuse of the upper head lol
                        bool therealbody = false;
                        foreach (var c in b.Children)
                        {
                            if (!therealbody)
                            {
                                therealbody = true;
                                continue;
                            }

                            CityDetail temp_detail = new CityDetail();
                            //this is the column
                            foreach (var d in c.Children)
                            {
                                //Console.WriteLine("even this is c: " + d.InnerHtml);
                                d.InnerHtml = d.InnerHtml.Replace("<br>", " "); // doesnt work, just dont forget to do this
                                d.InnerHtml = d.InnerHtml.Replace("&nbsp;", "");
                                //string tempstr = d.InnerHtml;
                                //int location = d.InnerHtml.IndexOf("&nbsp;");
                                //if(location > -1)
                                //{
                                //    d.InnerHtml = d.InnerHtml.Remove(location, 6);
                                //}
                                //d.i = d.InnerHtml.IndexOf("<br>")

                                //a.UniversalDetail.Add(d.InnerHtml);
                                temp_detail.UniversalDetail.Add(d.InnerHtml.HtmlEncode(Encoding.UTF8));
                                //string temp_str = d.InnerHtml.HtmlEncode(Encoding.UTF8);
                                header_str.Add(d.InnerHtml.ToString());
                            }

                            temporary_data.Add(temp_detail);
                        }
                        //int a = 10;
                    }
                    //table content
                    else if (itt == 3)
                    {

                        foreach (var c in b.Children)
                        {
                            //int itt1 = 0;

                            CityDetail temp_detail = new CityDetail();
                            bool justheader = false;
                            foreach (var d in c.Children)
                            {

                                //Console.WriteLine("even this is d: " + d.InnerHtml);
                                if (!justheader)
                                {
                                    justheader = true;
                                    foreach (var e in d.Children)
                                    {
                                        d.InnerHtml = d.InnerHtml.Replace("&nbsp;", "-");
                                        //Console.WriteLine("just for city: " + e.InnerHtml);
                                        temp_detail.UniversalDetail.Add(e.InnerHtml.HtmlEncode(Encoding.UTF8));
                                    }
                                    continue;
                                }

                                d.InnerHtml = d.InnerHtml.Replace("&nbsp;", "-");
                                //Console.WriteLine("even this is d: " + d.InnerHtml);
                                temp_detail.UniversalDetail.Add(d.InnerHtml.HtmlEncode(Encoding.UTF8));
                                //Console.WriteLine("itt1: " + itt1++);
                            }

                            temporary_data.Add(temp_detail);
                        }
                    }
                    itt++;
                }
            }

            int itterator_helper = 0;
            RBoxDisplay.AppendText($"{"-----FETCHED FROM ONLINE-----\n"}");
            foreach (var q in temporary_data)
            {
                string disp;
                disp = "\n" + itterator_helper + ". " + q.PrintInString(";");
                RBoxDisplay.AppendText($"{disp}");
                itterator_helper++;
            }

            //scrapping done. now move them into file
            WriteTheLists(temporary_data);
        }

        //private string getTime()
        //{
        //    string htmlResult = fetchedRes.InnerHtml.ReplaceFirst("<strong>기상실황표</strong>", "");
        //    Console.WriteLine("time result: " + htmlResult);

        //    htmlResult = htmlResult.Remove(htmlResult.Length, 3);

        //    return htmlResult;

        //    //DateTime local = DateTime.Now;
        //    //string localDateStr = local.ToString("yyyy-MM-dd-HH") + ".00";
        //    //return localDateStr;
        //}

        private void WriteTheLists(List<CityDetail> thelist)
        {
            //think about this later
            string thispath = Directory.GetCurrentDirectory();
            string filename = thispath + "\\output\\" + currentDateTime + ".txt";
            //string filename = getTime() + ".txt";
            //string[] arrFinalStr = new string[thelist.Count()];
            List<string> finalStr = new List<string>();
            foreach (var list in thelist)
            {
                finalStr.Add(list.PrintInString(";"));
            }

            File.WriteAllLines(filename, finalStr.ToArray());

            RBoxDisplay.AppendText($"{"\n\nsaved as " + finalStr}");
        }

        private void DoScraping(IHtmlDocument document)
        {
            IEnumerable<IElement> articleLink = null;
            articleLink = document.All.Where(x => x.ClassName == "table-col");
            //articleLink = document.QuerySelectorAll("tbody");

            if (articleLink.Any())
                PrintResults(articleLink);
            else
            {
                RBoxDisplay.AppendText($"{"-----------------------\nERROR: No data fetched, please check your internet connection\n\n"}");
            }

        }

        private string[] splitLocalFileRes(string fulltext)
        {
            string[] lines = fulltext.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return lines;
        }

        private void UseLocalData()
        {
            string thispath = Directory.GetCurrentDirectory();
            string filename = thispath + "\\output\\" + currentDateTime + ".00.txt";

            string readText = File.ReadAllText(filename);

            string[] collectedData = splitLocalFileRes(readText);

            RBoxDisplay.AppendText($"{"-----FETCHED FROM LOCAL DATA-----\n"}");
            foreach (var cd in collectedData)
            {
                RBoxDisplay.AppendText($"{cd}");
            }
        }

        private void GetScrapeResults(IHtmlDocument document)
        {
            //should be time validation
            // if the data from local has the same hour, so just take from the local. Else, take online
            if(NeedToDownload(document) == 1)
            {
                DoScraping(document);
            }
            else
            {
                UseLocalData();
            }

            //articleLink = document.All.Where(x => x.ClassName == "table-col" && x.ClassList.Contains("tbody"));

            //if (articleLink.Any())
            //{
            //    ProceedToTable(articleLink);
            //}

            //foreach (var term in QueryTerms)
            //{
            //    articleLink = document.All.Where(x =>
            //        x.ClassName == "views-field views-field-nothing" &&
            //        (x.ParentElement.InnerHtml.Contains(term) || x.ParentElement.InnerHtml.Contains(term.ToLower()))).Skip(1);

            //    //Overwriting articleLink above means we have to print it's result for all QueryTerms
            //    //Appending to a pre-declared IEnumerable (like a List), could mean taking this out of the main loop.
            //    if (articleLink.Any())
            //    {
            //        PrintResults(articleLink);
            //    }
            //}
        }
    }
}
