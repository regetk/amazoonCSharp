using System.Collections.Generic;
using System.IO;
using Amazoon.Models;
using System.Xml.Linq;

public class FindProd
{
    private static string AWS_KEY;
    private static string AWS_SECRET;
    private static string fName = @"C:\Users\Reget.Kalamees\Documents\NetBeansProjects\Amazoon2\konf.properties";
    private static string destination = "ecs.amazonaws.de";

    private string fWord;
    private int wpNr;

    private static Dictionary<string,string> ReadDictionaryFile(string fileName)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        foreach (string line in File.ReadAllLines(fileName))
        {
            if ((!string.IsNullOrEmpty(line)) &&
                (!line.StartsWith(";")) &&
                (!line.StartsWith("#")) &&
                (!line.StartsWith("'")) &&
                 (line.Contains("="))
                )
            {
                int index = line.IndexOf('=');
                string key = line.Substring(0, index).Trim();
                string value = line.Substring(index + 1).Trim();

               
                dictionary.Add(key, value);
            }
        }

        return dictionary;
    }

    private static void readConf() {
       Dictionary<string,string> cDic=ReadDictionaryFile(fName);
       bool ok = cDic.TryGetValue("keyId", out AWS_KEY);
       ok = cDic.TryGetValue("keySecret", out AWS_SECRET);

    }

    public FindProd(string fWord, string webPageNr) {
        if (AWS_KEY == null) readConf();
        this.fWord = fWord;
        this.wpNr = int.Parse(webPageNr);
    }

    

    private string makeSignedUrl(string wsPage){
        string requestString = "Service=AWSECommerceService"
                       + "&Version=2009-03-31"
                       + "&Operation=ItemSearch"
                       + "&AssociateTag=proovitoo-20"
                       + "&SearchIndex=Books"
                       + "&ResponseGroup=Small,OfferSummary"
                       + "&Keywords=" + this.fWord
                       + "&ItemPage="+wsPage
                       ;
       AmazonProductAdvtApi.SignedRequestHelper srh = new AmazonProductAdvtApi.SignedRequestHelper(AWS_KEY, AWS_SECRET, destination);
  
       string requestUrl = srh.Sign(requestString);
       return requestUrl;
    }

    public List<FoundItem> process(int sp, int ep,out string totalPages) {
        List<FoundItem> found = new List<FoundItem>();
        string sUrl = makeSignedUrl("1");
        XDocument xdoc=XDocument.Load(sUrl);
        //namespace !!!
        string nameSp = @"{http://webservices.amazon.com/AWSECommerceService/2011-08-01}";
        IEnumerable<XElement> totPages = xdoc.Root.Descendants(nameSp+"TotalPages");
        totalPages = "0";
        
        //only one TotalPages element
       foreach(XElement xe in totPages) totalPages =xe.Value;
        
        foreach (XElement xe in xdoc.Root.Descendants(nameSp + "Item"))
        {
            FoundItem fi = new FoundItem();
            fi.asin = xe.Element(nameSp+"ASIN").Value;
            fi.title = xe.Element(nameSp+"ItemAttributes").Element(nameSp+"Title").Value;
            IEnumerable<XElement> xePrices = xe.Descendants(nameSp+"Amount");
            if (xePrices != null) {
                foreach (XElement xeAmount in xePrices) fi.price = xeAmount.Value;
            }
            found.Add(fi);


        
        }


        return found;
    }

    

}