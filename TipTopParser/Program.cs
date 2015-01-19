using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace TipTopParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Run a = new Run();
            Console.WriteLine("Program finished - press anykay for close");
            Console.ReadLine();
        }

        class Run
        {
            // <div class="cat_title" id="cat_1">Комплектуючі для комп'ютерів</div>
            string titlesPars = "<div class=\"cat_title\" id=\"(.*?)\">(.*?)</div>(.*?)</a></div>";

            // <a href="/category/1/">Процесори</a>
            string subTitlesPars = "<a href=\"(.*?)/\">(.*?)</a>";

            //<a href=\"/product/(.*?)/\">"
            string productsPars = "<h2><a href=\"/product/(.*?)/\">";

            //     <a href=\"/category/(.*?)/brand/0/price/-/filter/-/page/(.*?)\">(.*?)</a></div>
            string lastPgNumberPars = "<a href=\"/category/(.*?)/brand/0/price/-/filter/-/page/(.*?)\">";

            //<div class=\"inner_page_title\"><h1 itemprop=\"name\">(.*?)</h1></div>
            string productNamePars = "<div class=\"inner_page_title\"><h1 itemprop=\"name\">(.*?)</h1></div>";

            //<div class=\"pi_val_price\"><span>(.*?) (.*?)</span>      // 1
            string productPricePars = "<div class=\"pi_val_price\"><span>(.*?)&nbsp;грн.</span>";

            //<a id="\img_main\" class=\"fancybox\" title=(.*?) href=\"(.*?)\"
            string productImgPars = "<a id=\"img_main\" (.*?) href=\"(.*?)\"";

            // BUG BUG BUG
            //<div id="tab2" class=\"tab-pane\"><p itemprop="description"></p><p>(.*?)</p><p></p></div>
            string productDescrPars = "";

            //<tr><td><div class=\"pi_key\"><span title=\"\">(.*?)</span></div></td><td><div class=\"pi_val_attr\">(.*?)</div></td></tr>  // 1 , 2
            string productGeneralPars = "<tr><td><div class=\"pi_key\"><span title=\"\">(.*?)</span></div></td><td><div class=\"pi_val_attr\">(.*?)</div></td></tr>";

            //<tr><td><div class=\"pi_key\">(.*?)</div></td><td><div class=\"pi_val\">(.*?)</div></td></tr>  // 1 , 2
            string productAttrsPars = "<tr><td><div class=\"pi_key\">(.*?)</div></td><td><div class=\"pi_val\">(.*?)</div></td></tr>";

            public Run()
            {
                string website = "http://tiptop.ua";

                foreach (Match ttl in Regex.Matches(getPage(website), titlesPars))
                {
                    Console.WriteLine(ttl.Groups[2].Value);
                    string allSubTitles = ttl.Groups[3].Value;
                    foreach (Match subTtl in Regex.Matches(allSubTitles, subTitlesPars))
                    {
                        Console.WriteLine("   - " + subTtl.Groups[2].Value);
                        string categoryPageUrl = website + subTtl.Groups[1].Value;
                        string categoryPage = getPage(categoryPageUrl);

                        Match m = Regex.Matches(categoryPage, lastPgNumberPars)[Regex.Matches(categoryPage, lastPgNumberPars).Count - 1];

                        int pageCount = Convert.ToInt32(m.Groups[2].Value);
                        for (int i = 1; i <= pageCount; i++)
                        {
                            string nextPage = categoryPageUrl + "/brand/0/price/-/filter/-/page/" + i;
                            foreach (Match product in Regex.Matches(getPage(nextPage), productsPars))
                            {
                                string productInfoUrl = website + "/product/" + product.Groups[1].Value;
                                string productInfoPage = getPage(productInfoUrl);

                                ProductInformation p = new ProductInformation();
                                p.name = Regex.Match(productInfoPage, productNamePars).Groups[1].Value;
                                p.price = Regex.Match(productInfoPage, productPricePars).Groups[1].Value;
                                p.img = website + Regex.Match(productInfoPage, productImgPars).Groups[2].Value;
                                //p.discription = Regex.Match(productInfoPage, productDescrPars).Value;
                                Console.WriteLine(p.img);
                                Console.WriteLine(p.name);
                                Console.WriteLine(p.price);
                               // Console.WriteLine("description: \n" + p.discription);

                                foreach (Match generalInfo in Regex.Matches(productInfoPage, productGeneralPars))
                                {
                                    KeyValuePair<string, string> kvpair = new KeyValuePair<string, string>(generalInfo.Groups[1].Value, generalInfo.Groups[2].Value);
                                    p.generalInfo.Add(kvpair.Key, kvpair.Value);
                                    Console.WriteLine(kvpair.Key + "    -    " + kvpair.Value);
                                }

                                foreach (Match attribute in Regex.Matches(productInfoPage, productAttrsPars))
                                {
                                    KeyValuePair<string, string> kvpair = new KeyValuePair<string, string>(attribute.Groups[1].Value, attribute.Groups[2].Value);
                                    p.attributes.Add(kvpair.Key, kvpair.Value);
                                    Console.WriteLine(kvpair.Key + "    -    " + kvpair.Value);
                                }
                                Thread.Sleep(1000000);
                            }
                        }

                    }
                }
            }

            string getPage(string url)
            {
                WebRequest rq = WebRequest.Create(url);

                WebResponse rp = rq.GetResponse();
                Stream dataStream = rp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);

                return reader.ReadToEnd();
            }
        }

        class ProductInformation
        {
            public string name;
            public string price;
            public string discription;
            public string img;
            public Dictionary<string, string> generalInfo;
            public Dictionary<string, string> attributes;

            public ProductInformation()
            {
                generalInfo = new Dictionary<string, string>();
                attributes = new Dictionary<string, string>();
            }
        }
    }
}
