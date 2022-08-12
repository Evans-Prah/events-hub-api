namespace Entities.Event
{
    public class EventComment
    {
        public int CommentId { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public List<CommentReply>? Replies { get; set; }
        public long NumberOfLikes { get; set; }
        public List<EventCommentLiker>? Likes { get; set; }
    }
}
