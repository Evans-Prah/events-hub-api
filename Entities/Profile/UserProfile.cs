﻿namespace Entities.Profile
{
    public class UserProfile
    {
        public string? ResponseMessage { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public long FollowingCount { get; set; }
        public long FollowersCount { get; set; }
        public List<UserPhoto>? Photos { get; set; }
    }
}
