namespace JevilNet.Services.CustomCommand;

public class CommandStorage
{
    public string Caller { get; set; }
    public string Output { get; set; }

    public CommandStorage()
    {
    }

    public CommandStorage(string caller, string output)
    {
        Caller = caller;
        Output = output;
    }
}