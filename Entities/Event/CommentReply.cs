namespace Entities.Event
{
    public class CommentReply
    {
        public string RepliedByUsername { get; set; }
        public string RepliedByDisplayName { get; set; }
        public string? MainPhoto { get; set; }
        public string Reply { get; set; }
        public long? NumberOfLikes { get; set; }
        public List<EventCommentLiker>? Likes { get; set; }
    }
}
