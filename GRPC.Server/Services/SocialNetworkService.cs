using Grpc.Core;

namespace GRPC.Services;

public class SocialNetworkService : Social.SocialBase
{
    private static readonly List<Post> Posts_db = new();
    private static readonly List<Comment> Comments_db = new();

    private static readonly Dictionary<string, List<Message>> messages = new();
    private object lock_object = new();

    public override async Task<PostID> AddPost(Post request, ServerCallContext context)
    {
        request.Id = new PostID { Id = Posts_db.Count };
        Posts_db.Add(request);
        return request.Id;
    }

    public override async Task<Empty> AddComment(Comment request, ServerCallContext context)
    {
        Comments_db.Add(request);
        return new Empty();
    }

    public override async Task<Post> NextPost(PostID request, ServerCallContext context)
    {
        if (Posts_db.Count == 0 || Posts_db.Count - 1 == request.Id) return new Post();

        return Posts_db[request.Id + 1];
    }

    public override async Task<Post> PrevPost(PostID request, ServerCallContext context)
    {
        if (Posts_db.Count == 0 || request.Id < 1) return new Post();

        return Posts_db[request.Id - 1];
    }

    public override async Task ReadComments(PostID request, IServerStreamWriter<Comment> responseStream,
        ServerCallContext context)
    {
        var comments = Comments_db.FindAll(comment => comment.Postid.Id == request.Id);
        foreach (var comment in comments) await responseStream.WriteAsync(comment);
    }

    public override async Task<Post> LikePost(PostID request, ServerCallContext context)
    {
        Posts_db[request.Id].Likes += 1;
        return Posts_db[request.Id];
    }

    public override async Task<Empty> SendMessage(Message request, ServerCallContext context)
    {
        if (!messages.ContainsKey(request.User.Username)) messages[request.User.Username] = new List<Message>();

        messages[request.User.Username].Add(request);
        return new Empty();
    }

    public override async Task CheckMessages(User request, IServerStreamWriter<Message> responseStream,
        ServerCallContext context)
    {
        var _messages = messages.GetValueOrDefault(request.Username, null);
        if (_messages is null) return;

        foreach (var msg in _messages) await responseStream.WriteAsync(msg);

        messages.Remove(request.Username);
    }
}