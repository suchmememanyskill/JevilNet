using Discord.Commands;

namespace JevilNet.Attributes;

public class TextCommandAttribute : CommandAttribute
{
    public string? Description { get; }
    public string FullCommandName { get; }

    public TextCommandAttribute() : base()
    { }

    public TextCommandAttribute(string text) : base(text)
    {
        FullCommandName = text;
    }

    public TextCommandAttribute(string text, bool ignoreExtraArgs) : base(text, ignoreExtraArgs)
    {
        FullCommandName = text;
    }

    public TextCommandAttribute(string text, string description) : this(text)
    {
        Description = description;
    }

    public TextCommandAttribute(string text, string description, bool ignoreExtraArgs) : this(text, ignoreExtraArgs)
    {
        Description = description;
    }
    
    public TextCommandAttribute(string text, string description, string fullCommandName) : this(text, description)
    {
        FullCommandName = fullCommandName;
    }

    public TextCommandAttribute(string text, string description, bool ignoreExtraArgs, string fullCommandName) : this(text, description, ignoreExtraArgs)
    {
        FullCommandName = fullCommandName;
    }
}