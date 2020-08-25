using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            foreach (var a in UniversalDetail)
            {
                Console.WriteLine(a);
            }
            Console.WriteLine("ended here -----------");
        }

        public string PrintInString(string delimiter)
        {
            string res = null;
            foreach (var a in UniversalDetail)
            {
                res += a + delimiter;
            }
            return res;
        }
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WebScraper());
        }
    }
}
