using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisioHub.Common.Models;

public class ServiceStatus
{
    public string Status { get; set; } = "-";
    public float CpuPerc { get; set; } = 0;
    public float Mem { get; set; } = 0;
    public string NetIO { get; set; } = "";


    public ServiceStatus()
    {

    }
    public ServiceStatus(string status)
    {
        var lines = status.Split("\n");
        for (int i = 0; i < lines.Length; i++)
            parseLine(lines[i], i);
    }

    private void parseLine(string line, int i)
    {
        if (line.Contains(":"))
        {
            var splitted = line.Split(":", 2);
            if (splitted[0].ToLower().Contains("status"))
                Status = splitted[1];
            else if (splitted[0].ToLower().Contains("cpu"))
            {
                var value = splitted[1].Trim();
                if (value.Contains("%"))
                    value = value.Substring(0, value.IndexOf("%")).Trim();
                CpuPerc = float.Parse(value);
            }
            else if (splitted[0].ToLower().Contains("mem"))
            {
                var value = splitted[1].Trim();
                if (value.Contains("%"))
                    value = value.Substring(0, value.IndexOf("%")).Trim();
                if (value.Contains("B"))
                    value = value.Substring(0, value.IndexOf("B")).Trim();
                if (value.Contains("M"))
                    value = value.Substring(0, value.IndexOf("M")).Trim();
                if (value.Contains("G"))
                    value = value.Substring(0, value.IndexOf("G")).Trim();
                Mem = float.Parse(value);
            }
            else if (splitted[0].ToLower().Contains("Net"))
            {
                var value = splitted[1].Trim();
                NetIO = value;
            }
        }
        else
        {
            if (i == 0)
                Status = line;
            else if (i == 1)
                CpuPerc = float.Parse(line);
            else if(i == 2)
                Mem = float.Parse(line);
        }
    }

    public override string? ToString()
    {
        return $"{nameof(ServiceStatus)} ({Status}, {CpuPerc}, {Mem})";
    }
}