// See https://aka.ms/new-console-template for more information

using Google.Protobuf.WellKnownTypes;
using GRPC;
using Grpc.Core;
using Grpc.Net.Client;

var channel = GrpcChannel.ForAddress("http://localhost:5189");

// var client = new Reverse.ReverseClient(channel);
//
// string message = "Doloribus neque exercitationem quos suscipit itaque aliquid repellendus.";
// Console.WriteLine("Send: " + message);
// var rev = await client.ReverseAsync(new ReverseRequest() { Message = message });
// Console.WriteLine("Recieve: " + rev.Message);

var client = new Social.SocialClient(channel);

Console.WriteLine("Username: ");
var user = Console.ReadLine();

var current_post = -1;
Task.Run(async () =>
{
    var u = new User { Username = user };
    while (true)
    {
        using (var comments = client.CheckMessages(u))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            await foreach (var comment in comments.ResponseStream.ReadAllAsync())
                Console.WriteLine("@{0}: {1}", comment.User.Username, comment.Message_);

            Console.ResetColor();
        }

        await Task.Delay(3000);
    }
});
while (true)
{
    Console.WriteLine(
        "a:<message> - add post, n - read next post, p - read previously post,+ - add like to this post, c: comment - add comment, cr - read comments");
    var line = Console.ReadLine();
    if (line.ToLower().StartsWith("a:"))
    {
        var post = new Post
        {
            Time = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            Username = user,
            Message = line.Substring(2)
        };
        var postId = await client.AddPostAsync(post);
        current_post = postId.Id;
        PrintPost(post);
        continue;
    }

    if (line.ToLower() == "n")
    {
        var post = await client.NextPostAsync(new PostID { Id = current_post });
        if (post.Id is null)
        {
            Console.WriteLine("No new posts!");
            continue;
        }

        current_post = post.Id.Id;
        PrintPost(post);
        continue;
    }

    if (line.ToLower() == "p")
    {
        var post = await client.PrevPostAsync(new PostID { Id = current_post });
        if (post.Id is null)
        {
            Console.WriteLine("It's first post!");
            continue;
        }

        current_post = post.Id.Id;
        PrintPost(post);
        continue;
    }

    if (line.ToLower().StartsWith("c:"))
    {
        var post = await client.AddCommentAsync(new Comment
        {
            Time = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            Username = user,
            Message = line.Substring(2),
            Postid = new PostID { Id = current_post }
        });
        continue;
    }

    if (line.ToLower().StartsWith("cr"))
        using (var comments = client.ReadComments(new PostID { Id = current_post }))
        {
            await foreach (var comment in comments.ResponseStream.ReadAllAsync()) PrintComment(comment);
        }

    if (line == "+")
    {
        var post = await client.LikePostAsync(new PostID { Id = current_post });
        PrintPost(post);
        continue;
    }

    if (line.StartsWith("@"))
    {
        var strings = line[1..].Split(':');
        await client.SendMessageAsync(new Message
        {
            User = new User { Username = strings[0] },
            Message_ = strings[1]
        });
    }
}

void PrintPost(Post post)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("******\n{0}\n\n {2}\n\nAuthor:{1}\tLikes: {3}\n******", post.Time, post.Username, post.Message,
        post.Likes);
    Console.ResetColor();
}

void PrintComment(Comment comment)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("{0}| {1} say: {2}", comment.Time, comment.Username, comment.Message);
    Console.ResetColor();
}