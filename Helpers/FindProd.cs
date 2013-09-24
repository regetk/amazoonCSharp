using System.Collections.Generic;
using System.IO;
using Amazoon.Models;
using System.Xml.Linq;
using System;

public class FindProd
{
    private static string AWS_KEY;
    private static string AWS_SECRET;
    private static string fName = @"C:\Users\Reget.Kalamees\Documents\NetBeansProjects\Amazoon2\konf.properties";
    private static string destination = "ecs.amazonaws.de";
    private static int wsPageSize = 10;  //web service page size

    private string fWord;
    private int wpNr;
    private int totalResults=0;

    public int getTotalResults() { return this.totalResults; }

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
        queryTotalResults();
    }

    private int queryTotalResults() {
        string sUrl = makeSignedUrl("1");
        XDocument xdoc = XDocument.Load(sUrl);
        //namespace !!!
        string nameSp = @"{http://webservices.amazon.com/AWSECommerceService/2011-08-01}";
        IEnumerable<XElement> totRes = xdoc.Root.Descendants(nameSp + "TotalResults");
         //only one TotalResults element
        foreach (XElement xe in totRes) this.totalResults = Convert.ToInt32(xe.Value);
        return this.totalResults;
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

    private void computeNeededPages(int wpPagenr, int elemsInWp, out int fWsPage, out int lWsPage, out int firstElementInWsFirstPage, out int lastElementInWsLastPage)
    {
        
        int wpFirstElemNr = (wpPagenr - 1) * elemsInWp; //first element index need to query (base 0)
        int wpLastElemNr = wpFirstElemNr + elemsInWp - 1; //last element index need to query (base 0)
        //adapt when in last page
        if (wpLastElemNr >= totalResults) wpLastElemNr = totalResults - 1;
        fWsPage = (wpFirstElemNr / wsPageSize) + 1; //first needed WS page nr (base 1)
        lWsPage = (wpLastElemNr / wsPageSize) + 1; //last needed WS page (base 1)
        firstElementInWsFirstPage = wpFirstElemNr % wsPageSize; //base 0
        lastElementInWsLastPage = wpLastElemNr % wsPageSize; //base 0
      }

    public List<FoundItem> process() {
        List<FoundItem> found = new List<FoundItem>();
        //namespace !!!
        string nameSp = @"{http://webservices.amazon.com/AWSECommerceService/2011-08-01}";
        int fWsPage=0;
        int lWsPage=0;
        int feInFWsPage=0;
        int leInLWsPage=0;

        computeNeededPages(this.wpNr,13, out fWsPage, out lWsPage, out feInFWsPage, out leInLWsPage);
        for (int a = fWsPage; a <= lWsPage; a++)
        {
            string sUrl = makeSignedUrl(a.ToString());
            XDocument xdoc = XDocument.Load(sUrl);
            //TODO - start and end idx
            foreach (XElement xe in xdoc.Root.Descendants(nameSp + "Item"))
            {
                FoundItem fi = new FoundItem();
                fi.asin = xe.Element(nameSp + "ASIN").Value;
                fi.title = xe.Element(nameSp + "ItemAttributes").Element(nameSp + "Title").Value;
                IEnumerable<XElement> xePrices = xe.Descendants(nameSp + "Amount");
                if (xePrices != null)
                {
                    foreach (XElement xeAmount in xePrices) fi.price = xeAmount.Value;
                }
                found.Add(fi);



            }
        } //for query each page

        return found;
    }

    

}