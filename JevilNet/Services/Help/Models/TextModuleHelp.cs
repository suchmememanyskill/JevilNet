using JevilNet.Attributes;

namespace JevilNet.Services.Help.Models;

public class TextModuleHelp
{
    public List<TextCommandHelp> Commands { get; } = new();
    private Type moduleType;

    public TextModuleHelp(Type module)
    {
        moduleType = module;
        GetCommands();
    }

    public void GetCommands()
    {
        moduleType.GetMethods().ToList().ForEach(x =>
        {
            TextCommandAttribute commandAttribute =
                (TextCommandAttribute) Attribute.GetCustomAttribute(x, typeof(TextCommandAttribute));

            if (commandAttribute != null)
            {
                Commands.Add(new TextCommandHelp(commandAttribute, x));
            }
        });
    }
    
    public override string ToString()
    {
        string temp = $"[{moduleType.Name}]\n";
        temp += String.Join("\n", Commands);
        return temp;
    }
}