﻿using Entities.Profile;

namespace Entities.UserFollowing
{
    public class Followings
    {
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public bool Following { get; set; }
        public long FollowingCount { get; set; }
        public long FollowersCount { get; set; }
        public List<UserPhoto>? Photos { get; set; }
    }
}
