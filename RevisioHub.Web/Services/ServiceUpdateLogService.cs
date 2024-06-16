namespace RevisioHub.Web.Services;

public class ServiceUpdateLogService
{
    public event Func<int, string, Task>? OnUpdateLog;


    public async Task UpdateLog(int LogId, string output)
    {
        if (OnUpdateLog != null)
            await OnUpdateLog.Invoke(LogId, output);
    }

}
