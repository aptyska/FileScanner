// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

Regex creditRegex = new Regex("(^4[0-9]{12}(?:[0-9]{3})?$)|(^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$)|(3[47][0-9]{13})|(^3(?:0[0-5]|[68][0-9])[0-9]{11}$)|(^6(?:011|5[0-9]{2})[0-9]{12}$)|(^(?:2131|1800|35\\d{3})\\d{11}$)");
Regex socialRegex = new Regex("^(\\d{3}-?\\d{2}-?\\d{4}|XXX-XX-XXXX)$");

ConcurrentBag<ScanResult> results = new ConcurrentBag<ScanResult>();
object resultLock = new object();

string sDir = Directory.GetCurrentDirectory();
if(args.Length > 0)
{
    sDir = args[0];
}

var extensions = new List<string> { ".txt", ".xml", ".html", ".config",".json" };
string[] files = Directory.GetFiles(sDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.IndexOf(Path.GetExtension(f)) >= 0).ToArray();

if (files.Length > 0)
    foreach (var file in files)
    {

        foreach (var line in File.ReadLines(file).AsParallel())
        {
            var creditMatches = creditRegex.Matches(line);
            if (creditMatches.Count > 0)
            {
                GetMatch(file, "Credit Card", creditMatches);
            }
            var socialMatches = socialRegex.Matches(line);
            if (socialMatches.Count > 0)
            {
                GetMatch(file, "Social", socialMatches);
            }
        }
    }

if(results.Count > 0)
    foreach (var result in results)
    Console.WriteLine(result.ToString());

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

    }
}