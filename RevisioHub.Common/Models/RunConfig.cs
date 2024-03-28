using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisioHub.Common.Models;
public class RunConfig
{
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, string?> EnvironmentVariables { get; set;} = new ();
    public HostType HostType { get; set; }
}