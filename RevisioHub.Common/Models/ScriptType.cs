namespace RevisioHub.Common.Models;

public enum ScriptType
{
    Unknown,
    StartUp,
    ShutDown,
    Reset,
    Status,
    Update,
    CleanCache,
    Install,
}