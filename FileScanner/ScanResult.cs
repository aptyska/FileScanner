// See https://aka.ms/new-console-template for more information
using System.IO.Enumeration;

internal class ScanResult
{

    public string? FileName {get;set;}

    public string? ResultType { get; set; }

    public string? Value { get; set; }

    public override string ToString()
    {
        return "Filename " + FileName + ": " + " Contains " + ResultType + ": " + Value + Environment.NewLine;
    }

}