
using FileScanner;
using Microsoft.Office.Interop.Word;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

//Pull credit card numbers if they are at the start or end of a file or have a space around the digits
Regex creditRegex = new Regex("((^| )4[0-9]{12}(?:[0-9]{3})?($| ))|((^| )(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}($| ))|(3[47][0-9]{13})|((^| )3(?:0[0-5]|[68][0-9])[0-9]{11}($| ))|((^| )6(?:011|5[0-9]{2})[0-9]{12}($| ))|((^| )(?:2131|1800|35\\d{3})\\d{11}($| ))");
//Pull social numbers if they are at the beginning or end of a file, are a continous group of numbers or seperated by dashes
Regex socialRegex = new Regex("(^| )(?!0{3})(?!6{3})[0-8]\\d{2}-?(?!0{2})\\d{2}-?(?!0{4})\\d{4}($| )");

ConcurrentBag<ScanResult> results = new ConcurrentBag<ScanResult>();
object resultLock = new object();
bool SearchWordDocs = false;

string sDir = Directory.GetCurrentDirectory();
if(args.Length > 0)
{
    if (args[0] != "-w")
        sDir = args[0];
    if(args.Contains<string>("-w"))//skip word docs by default because they are slow
    {
       SearchWordDocs = true;
    }
}

FileSearch textFileSearch = new FileSearch(new List<string> { ".txt" });

var textExtensions = new List<string> { ".txt", ".xml", ".html", ".config",".json" };//pull in raw text file types
string[] textFiles = textFileSearch.GetFiles(sDir);

if (textFiles?.Length > 0)
    Parallel.ForEach<string>(textFiles, file => ParseTextFile(file));//parse files in paralell for speed


if(SearchWordDocs)//only search for word docs if asked, searching word docs is slow
{
    FileSearch wordFileSearch = new FileSearch(new List<string> { ".doc", ".docx" });//pull in word document types
    string[] wordFiles = wordFileSearch.GetFiles(sDir);
    if (wordFiles?.Length > 0)
        Parallel.ForEach<string>(wordFiles, file => ParseWordFile(file));
}



if(results?.Count > 0)
{  
    foreach (ScanResult result in results)
    {
        File.AppendAllText("Results.log", result.ToString());
    }
}

Console.WriteLine("Please Press Enter to Quit");
Console.ReadLine();



void ParseWordFile(string file)
{
    // Create an application object if the passed in object is null
    Application winword = new Application();

    // Use the application object to open our word document in ReadOnly mode
    Document wordDoc = winword.Documents.Open(file, ReadOnly: true);
    string text = wordDoc.Content.Text;
    string result = Regex.Replace(text, @"\r\n?|\n", " ");//format text from document for regex

    ParseText(file, result);

}

void GetMatch(string file, string type, MatchCollection matches)
{
    foreach (Match credit in matches)
    {
        ScanResult scanResult = new ScanResult
        {
            FileName = file,
            ResultType = type,
            Value = credit.Value

        };

        lock (resultLock)
        {
            results.Add(scanResult);
        }
        Console.Write(scanResult.ToString());

    }
}

void ParseTextFile(string file)
{
    foreach (var line in File.ReadLines(file))
    {
        ParseText(file, line);
    }
}

void ParseText(string filename,string text)
{
    var creditMatches = creditRegex.Matches(text);
    if (creditMatches.Count > 0)
    {
        GetMatch(filename, "Credit Card", creditMatches);
    }
    var socialMatches = socialRegex.Matches(text);
    if (socialMatches.Count > 0)
    {
        GetMatch(filename, "Social", socialMatches);
    }
}