using System.Reflection;
using JevilNet.Attributes;

namespace JevilNet.Services.Help.Models;

public class TextCommandHelp
{
    public TextCommandAttribute Command { get; }
    public MethodInfo MethodInfo { get; }

    private List<string> args;

    public string CommandName => Command.FullCommandName;
    public string? CommandDescription => Command.Description;

    public TextCommandHelp(TextCommandAttribute command, MethodInfo info)
    {
        Command = command;
        MethodInfo = info;

        args = MethodInfo.GetParameters().ToList().OrderBy(x => x.Position).Select(x => x.Name).ToList();
    }

    public override string ToString()
    {
        string temp = "";
        if (CommandName != null)
            temp += CommandName + " ";

        if (args.Count > 0)
            temp += $"{{{string.Join("} {", args)}}} ";

        if (CommandDescription != null)
            temp += $"| {CommandDescription}";

        return temp.Trim();
    }
}