﻿using System.ComponentModel.DataAnnotations;

namespace RevisioHub.Common.Models.db;

public class Service
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ServiceScript> ServiceScripts { get; set; } = new();
    public List<ServiceHost> ServiceHosts { get; set; } = new();

    public List<InstallationFile> InstallationFiles { get; set; } = new();
    public List<ServiceEnvironmentVariable> EnvironmentVariables { get; set; } = new();

}
