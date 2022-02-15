using JevilNet.Services.Vote;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Attributes;

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

public class VoteActiveAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        VoteService service = services.GetRequiredService<VoteService>();

        if (context.Guild == null)
            return Task.FromResult(PreconditionResult.FromError("Not in a guild"));
        
        if (service.GetModel(context.Guild.Id).Active)
            return Task.FromResult(PreconditionResult.FromSuccess());
        
        return Task.FromResult(PreconditionResult.FromError("No vote is currently active"));
    }
}

public class VoteInactiveAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        VoteService service = services.GetRequiredService<VoteService>();

        if (context.Guild == null)
            return Task.FromResult(PreconditionResult.FromError("Not in a guild"));
        
        if (!service.GetModel(context.Guild.Id).Active)
            return Task.FromResult(PreconditionResult.FromSuccess());
        
        return Task.FromResult(PreconditionResult.FromError("A vote is currently active"));
    }
}