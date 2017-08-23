using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Web;

namespace RZUEP_CA
{
    class Program
    {
        static bool FirstPlan = true;
        static bool FirstPro = true;
        static void Main(string[] args)
        {
            HttpWebRequest open = (HttpWebRequest)WebRequest.Create(@"http://rzuep.apphb.com/AktualizacjaZmiana?status=true&key=*HIDDEN*");
            open.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)open.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                if(reader.ReadToEnd()!="True")
                {
                    Console.WriteLine("Open Aktualizacja NOT TRUE");
                }
            }

            HtmlDocument doc = null;
            HtmlWeb website = new HtmlWeb();
            doc = website.Load("http://ue.poznan.pl/pl/studenci,c172/semestralne-plany-zajec,c1345/studia-stacjonarne-i-niestacjonarne-wieczorowe-i-stopnia,c1336/");
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (link.GetAttributeValue("href", null).Contains("http://rozkladyzajec.ue.poznan.pl/")) WydzialToPlan("I. stopnia", link.GetAttributeValue("title", null).Replace("Wydział ",""), link.GetAttributeValue("href", null));
            }

            HtmlWeb website2 = new HtmlWeb();
            doc = website2.Load("http://ue.poznan.pl/pl/studenci,c172/semestralne-plany-zajec,c1345/studia-stacjonarne-i-niestacjonarne-wieczorowe-ii-stopnia,c1337/");
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (link.GetAttributeValue("href", null).Contains("http://rozkladyzajec.ue.poznan.pl/")) WydzialToPlan("II. stopnia", link.GetAttributeValue("title", null).Replace("Wydział ",""), link.GetAttributeValue("href", null));
            }

            HtmlWeb website3 = new HtmlWeb();
            doc = website3.Load("http://ue.poznan.pl/pl/pracownicy,c359/studia-stacjonarne-i-niestacjonarne-wieczorowe,c495/");
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (link.GetAttributeValue("href", null).Contains("http://rozkladyzajec.ue.poznan.pl/")) WydzialToProwadzacy(link.GetAttributeValue("href", null), link.GetAttributeValue("title", null).Replace("Wydział ", ""));
            }

            HttpWebRequest close = (HttpWebRequest)WebRequest.Create(@"http://rzuep.apphb.com/AktualizacjaZmiana?status=false&key=*HIDDEN*");
            close.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)close.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                if (reader.ReadToEnd() != "False")
                {
                    Console.WriteLine("Close Aktualizacja NOT FALSE");
                }
            }
        }

        static void WydzialToPlan(string stopien, string wydzial, string url)
        {
            HtmlWeb website = new HtmlWeb();
            website.OverrideEncoding = Encoding.GetEncoding(1250);
            var inner = website.Load(url).DocumentNode.InnerHtml;
            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(inner, myWriter);
            var clearinner = myWriter.ToString();
            var doc = new HtmlDocument();
            doc.LoadHtml(clearinner);
            foreach (HtmlNode form in doc.DocumentNode.SelectNodes("//form"))
            {
                var rok = form.NextSibling.NextSibling.InnerHtml;
                var posturl = form.GetAttributeValue("action", null);
                foreach (HtmlNode option in form.ParentNode.ChildNodes["select"].SelectNodes("option"))
                {
                    var wyklist = option.NextSibling.InnerText;
                    var kierunek = wyklist.Split(new string[] { " Rok" }, StringSplitOptions.None)[0].Substring(1);
                    string grupaspl = wyklist.Contains("grupa ") ? "grupa " : "Grupa ";
                    var grupastr = wyklist.Split(new string[] { grupaspl }, StringSplitOptions.None)[1].Substring(0,2);
                    var grupaleft = wyklist.Split(new string[] { grupaspl }, StringSplitOptions.None)[0];
                    var specjalnosc = grupaleft.Split(new string[] { "I " }, StringSplitOptions.None)[1];
                    if (specjalnosc.Length > 0)
                    {
                        specjalnosc = specjalnosc.Remove(specjalnosc.Length - 1, 1);
                        if (specjalnosc[0] == ' ') specjalnosc = specjalnosc.Substring(1);
                    }
                    PlanToModel(stopien, wydzial, rok, kierunek, grupastr, specjalnosc, wyklist, posturl);
                }
            }
        }
        static void PlanToModel(string stopien, string wydzial, string rok, string kierunek, string grupa, string specjalnosc, string wyklist, string posturl)
        {
            RZUEPDBContext db = new RZUEPDBContext();
            string responseString;
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["WykList"] = wyklist;
                values["GoButton"] = "Go";
                var response = client.UploadValues(posturl, values);

                StringWriter myWriter = new StringWriter();
                HttpUtility.HtmlDecode(Encoding.Default.GetString(response), myWriter);
                responseString = myWriter.ToString();
            }
            var website = new HtmlDocument();
            website.LoadHtml(responseString.Replace("\r", "").Replace("\n", ""));
            var inner = website.DocumentNode.InnerHtml;
            StringWriter myWriter2 = new StringWriter();
            HttpUtility.HtmlDecode(inner, myWriter2);
            var clearinner = myWriter2.ToString();
            var clear = new HtmlDocument();
            clear.LoadHtml(clearinner);
            var doc = website.DocumentNode;
            var semestr = doc.SelectSingleNode("//cite").FirstChild.InnerText;

            if(FirstPlan)
            {
                var dbplans = db.Plans;
                foreach (var pl in dbplans.Where(x => x.semestr == semestr).ToList())
                {
                    var plzajecia = pl.Zajecia;
                    foreach (var z in plzajecia.ToList())
                    {
                        var zprowadzacy = z.Prowadzący;
                        foreach (var pr in zprowadzacy.ToList())
                        {
                            db.Prowadzacys.Remove(pr);
                        }
                        db.SaveChanges();
                        db.Zajecias.Remove(z);
                    }
                    db.SaveChanges();
                    //db.Plans.Remove(pl);
                }
                //db.SaveChanges();
                FirstPlan = false;
            }
            semestr = Clear(semestr);
            stopien = Clear(stopien);
            wydzial = Clear(wydzial);
            rok = Clear(rok);
            kierunek = Clear(kierunek);
            grupa = Clear(grupa);
            specjalnosc = Clear(specjalnosc);
            int planid = 0;
            if (db.Plans.Where(x => x.semestr == semestr && x.stopien == stopien && x.wydzial == wydzial && x.rok == rok && x.kierunek == kierunek && x.grupa == grupa && x.specjalnosc == specjalnosc).ToList().Count() > 0)
            {
                planid = db.Plans.Where(x => x.semestr == semestr && x.stopien == stopien && x.wydzial == wydzial && x.rok == rok && x.kierunek == kierunek && x.grupa == grupa && x.specjalnosc == specjalnosc).ToList().First().id;
            }
            else
            {
                var dodajplan = db.Plans.Add(new Plan { semestr = semestr, tryb = "Stacjonarne", stopien = stopien, wydzial = wydzial, rok = rok, kierunek = kierunek, grupa = grupa, specjalnosc = specjalnosc });
                db.SaveChanges();
                planid = dodajplan.id;
            }
            if(planid==0)
            {
                Console.WriteLine("planid=0!");
            }
            List<string> dodinforows = new List<string>();
            foreach (var row in doc.SelectNodes("//tbody").Last().ChildNodes.Where(x => x.Name == "tr"))
            {
                dodinforows.Add(row.InnerText[0] == '(' ? row.InnerText : "(" + row.InnerText);
            }

            foreach (var row in doc.SelectSingleNode("//tbody").ChildNodes)
            {
                var days = row.SelectNodes("td");
                if(days.Count!=5)
                {
                    Console.WriteLine("Liczba dni!");
                }
                for(int i=0; i<5; i++)
                {
                    var innerinfo = days[i].InnerHtml.Substring(1);
                    while (innerinfo.Contains("  ")) innerinfo = innerinfo.Replace("  "," ");
                    if (innerinfo.Equals(" ")) continue;
                    var infos = innerinfo.Split(new string[] { "<br> " }, StringSplitOptions.None);
                    var godzinaod = infos[0].Split('-')[0];
                    var godzinado = infos[0].Split('-')[1];
                    var dodinfo = "";
                    if (godzinado.Contains(" # "))
                    {
                        godzinado = godzinado.Split(new string[] { " # " }, StringSplitOptions.None)[0];
                        string[] dodpoints = infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1].Contains(',') ? infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1].Split(',') : new string[] { infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1] };
                        for(int d=0; d<dodpoints.Length; d++)
                        {
                            if(d>0) dodinfo += "<br>";
                            dodinfo += dodinforows.Where(x => x.Contains(dodpoints[d])).First().Split(new string[] { ") " }, StringSplitOptions.None)[1].Replace("(","").Replace(")", "");
                        }
                    }
                    var zajecia = infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[0];
                    var rodzaj = infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[1].Remove(infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[1].Length-1);
                    switch(rodzaj)
                    {
                        case "Wyk":
                            rodzaj = "Wykład";
                            break;
                        case "Ćwi":
                            rodzaj = "Ćwiczenia";
                            break;
                        case "Lek":
                            rodzaj = "Lektorat";
                            break;
                        case "Lab":
                            rodzaj = "Laboratoria";
                            break;
                        default:
                            Console.WriteLine("Rodzaj zajęć!");
                            break;
                    }
                    var sala = "";
                    if (infos[2].Equals("harmonogram")) sala = "Harmonogram w Hornecie";
                    else sala = infos[2];
                    var dodajzajecia = db.Zajecias.Add(new Zajecia { planid = planid, dzien = i, godzinaod = Clear(godzinaod.Replace('.', ':')), godzinado = Clear(godzinado.Replace('.',':')), rodzaj = Clear(rodzaj), nazwa = Clear(zajecia.Replace(".", ". ")), sala = Clear(sala), info = Clear(dodinfo) });
                    db.SaveChanges();
                    if (!infos[2].Equals("harmonogram"))
                    {
                        int inf = 3;
                        while (inf < infos.Length  && (infos[inf].Contains("Mgr") || infos[inf].Contains("Dr") || infos[inf].Contains("Prof")))
                        {
                            var dodajprowadzacy = db.Prowadzacys.Add(new Prowadzacy { zajeciaid = dodajzajecia.id, nazwa = Clear(infos[inf].Replace(".", ". ")) });
                            db.SaveChanges();
                            inf++;
                        }
                    }
                }
            }
        }

        static void WydzialToProwadzacy(string url, string wydzial)
        {
            HtmlWeb website = new HtmlWeb();
            website.OverrideEncoding = Encoding.GetEncoding(1250);
            var inner = website.Load(url).DocumentNode.InnerHtml;
            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(inner, myWriter);
            var clearinner = myWriter.ToString();
            var doc = new HtmlDocument();
            doc.LoadHtml(clearinner);
            foreach (HtmlNode form in doc.DocumentNode.SelectNodes("//form"))
            {
                var jednostka = form.NextSibling.NextSibling.InnerHtml;
                var posturl = form.GetAttributeValue("action", null);
                foreach (HtmlNode option in form.ParentNode.ChildNodes["select"].SelectNodes("option"))
                {
                    var wyklist = option.NextSibling.InnerText;
                    if (wyklist.Contains("hornecie") || wyklist.Contains("internecie") || wyklist.Contains("zespół")) continue;
                    ProwadzacyToModel(wydzial, jednostka, wyklist, posturl);
                }
            }
        }

        static void ProwadzacyToModel(string wydzial, string jednostka, string wyklist, string posturl)
        {
            RZUEPDBContext db = new RZUEPDBContext();
            string responseString;
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["WykList"] = wyklist;
                values["GoButton"] = "Go";
                var response = client.UploadValues(posturl, values);

                StringWriter myWriter = new StringWriter();
                HttpUtility.HtmlDecode(Encoding.Default.GetString(response), myWriter);
                responseString = myWriter.ToString();
            }
            var website = new HtmlDocument();
            website.LoadHtml(responseString.Replace("\r", "").Replace("\n", ""));
            var inner = website.DocumentNode.InnerHtml;
            StringWriter myWriter2 = new StringWriter();
            HttpUtility.HtmlDecode(inner, myWriter2);
            var clearinner = myWriter2.ToString();
            var clear = new HtmlDocument();
            clear.LoadHtml(clearinner);
            var doc = website.DocumentNode;
            var semestr = doc.SelectSingleNode("//cite").FirstChild.InnerText;

            if (FirstPro)
            {
                var dbproprowadzacys = db.Proprowadzacys;
                foreach (var p in dbproprowadzacys.Where(x => x.semestr == semestr).ToList())
                {
                    var pprozajecia = p.Prozajecia;
                    foreach (var z in pprozajecia.ToList())
                    {
                        var zgrupy = z.Grupy;
                        foreach (var g in zgrupy.ToList())
                        {
                            db.Grupys.Remove(g);
                        }
                        db.SaveChanges();
                        db.Prozajecias.Remove(z);
                    }
                    db.SaveChanges();
                    //db.Proprowadzacys.Remove(p);
                }
                //db.SaveChanges();
                FirstPro = false;
            }

            var nazwaprowadzacy = doc.SelectSingleNode("/html/body/font/table/caption/strong").InnerText;
            while (nazwaprowadzacy.Contains("  ")) nazwaprowadzacy = nazwaprowadzacy.Replace("  ", " ");
            nazwaprowadzacy = nazwaprowadzacy.Split(new string[] { " (" }, StringSplitOptions.None)[0];
            while (nazwaprowadzacy[0] == ' ') nazwaprowadzacy = nazwaprowadzacy.Substring(1);
            semestr = Clear(semestr);
            wydzial = Clear(wydzial);
            jednostka = Clear(jednostka);
            nazwaprowadzacy = Clear(nazwaprowadzacy);
            int proprowadzacyid = 0;
            if (db.Proprowadzacys.Where(x => x.semestr == semestr && x.wydzial == wydzial && x.jednostka == jednostka && x.nazwa == nazwaprowadzacy).ToList().Count() > 0)
            {
                proprowadzacyid = db.Proprowadzacys.Where(x => x.semestr == semestr && x.wydzial == wydzial && x.jednostka == jednostka && x.nazwa == nazwaprowadzacy).ToList().First().id;
            }
            else
            {
                var dodajproprowadzacy = db.Proprowadzacys.Add(new Proprowadzacy { semestr = semestr, tryb = "Stacjonarne", wydzial = wydzial, jednostka = jednostka, nazwa = nazwaprowadzacy });
                db.SaveChanges();
                proprowadzacyid = dodajproprowadzacy.id;
            }
            if(proprowadzacyid==0)
            {
                Console.WriteLine("proprowadzacyid=0!");
            }
            if (doc.InnerText.Contains("nie ma zajęć!")) return;
            List<string> dodinforows = new List<string>();
            foreach (var row in doc.SelectNodes("//tbody").Last().ChildNodes.Where(x => x.Name == "tr"))
            {
                dodinforows.Add(row.InnerText[0] == '(' ? row.InnerText : "(" + row.InnerText);
            }

            foreach (var row in doc.SelectSingleNode("//tbody").ChildNodes)
            {
                var days = row.SelectNodes("td");
                if (days.Count != 5)
                {
                    Console.WriteLine("Liczba dni!");
                }
                for (int i = 0; i < 5; i++)
                {
                    var innerinfo = days[i].InnerHtml.Substring(1);
                    while (innerinfo.Contains("  ")) innerinfo = innerinfo.Replace("  ", " ");
                    if (innerinfo.Equals(" ")) continue;
                    var infos = innerinfo.Split(new string[] { "<br> " }, StringSplitOptions.None);
                    var godzinaod = infos[0].Split('-')[0];
                    var godzinado = infos[0].Split('-')[1];
                    var dodinfo = "";
                    if (godzinado.Contains(" # "))
                    {
                        godzinado = godzinado.Split(new string[] { " # " }, StringSplitOptions.None)[0];
                        string[] dodpoints = infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1].Contains(',') ? infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1].Split(',') : new string[] { infos[0].Split('-')[1].Split(new string[] { " # " }, StringSplitOptions.None)[1] };
                        for (int d = 0; d < dodpoints.Length; d++)
                        {
                            if (d > 0) dodinfo += "<br>";
                            dodinfo += dodinforows.Where(x => x.Contains(dodpoints[d])).First().Split(new string[] { ") " }, StringSplitOptions.None)[1].Replace("(", "").Replace(")", "");
                        }
                    }
                    var zajecia = infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[0];
                    var rodzaj = infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[1].Remove(infos[1].Split(new string[] { " (" }, StringSplitOptions.None)[1].Length - 1);
                    switch (rodzaj)
                    {
                        case "Wyk":
                            rodzaj = "Wykład";
                            break;
                        case "Ćwi":
                            rodzaj = "Ćwiczenia";
                            break;
                        case "Lek":
                            rodzaj = "Lektorat";
                            break;
                        case "Lab":
                            rodzaj = "Laboratoria";
                            break;
                        default:
                            Console.WriteLine("Rodzaj zajęć!");
                            break;
                    }
                    var sala = infos[2];
                    var dodajprozajecia = db.Prozajecias.Add(new Prozajecia { proprowadzacyid = proprowadzacyid, dzien = i, godzinaod = Clear(godzinaod.Replace('.', ':')), godzinado = Clear(godzinado.Replace('.', ':')), rodzaj = Clear(rodzaj), nazwa = Clear(zajecia.Replace(".", ". ")), sala = Clear(sala), info = Clear(dodinfo) });
                    db.SaveChanges();
                    int inf = 3;
                    while (inf < infos.Length && (infos[inf].Contains(" R")))
                    {
                        var dodajgrupy = db.Grupys.Add(new Grupy { prozajeciaid = dodajprozajecia.id, nazwa = Clear(infos[inf]) });
                        db.SaveChanges();
                        inf++;
                    }
                    
                }
            }
        }

        static string Clear(string input)
        {
            if (input!="" && input!=null)
            {
                while (input.Contains("  ")) input = input.Replace("  ", " ");
                while (input[0] == ' ') input = input.Substring(1);
                while (input[input.Length - 1] == ' ') input = input.Remove(input.Length - 1);
            }
            return input;
        }
    }
}
