using Grpc.Core;

namespace GRPC.Services;

public class ReverseService : Reverse.ReverseBase
{
    private readonly ILogger<ReverseService> _logger;

    public ReverseService(ILogger<ReverseService> logger)
    {
        _logger = logger;
    }

    public override async Task<ReverseResponse> Reverse(ReverseRequest request, ServerCallContext context)
    {
        return new ReverseResponse { Message = new string(request.Message.Reverse().ToArray()) };
    }
}