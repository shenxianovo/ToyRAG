using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;

namespace ToyRAG.Cli.Utils
{
    public class Middleware
    {
        public static async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
        {
            StringBuilder functionCallDetails = new();
            functionCallDetails.Append($"\nTool Call: '{context.Function.Name}'");
            if (context.Arguments.Count > 0)
            {
                functionCallDetails.Append($"\n\tArgs: {string.Join(",", context.Arguments.Select(x => $"\n\t\t[{x.Key} = {x.Value}]"))}");
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(functionCallDetails);
            Console.ForegroundColor = ConsoleColor.White;

            return await next(context, cancellationToken);
        }
    }
}
