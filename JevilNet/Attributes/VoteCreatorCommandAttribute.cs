using Discord.Commands;
using JevilNet.Services.Vote;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Attributes;

public class VoteCreatorAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        VoteService service = services.GetRequiredService<VoteService>();

        if (context.Guild == null)
            return Task.FromResult(PreconditionResult.FromError("Not in a guild"));
        
        if (service.IsCreator(context.Guild.Id, context.User.Id))
            return Task.FromResult(PreconditionResult.FromSuccess());
        
        return Task.FromResult(PreconditionResult.FromError("Not the creator of the vote!"));
    }
}