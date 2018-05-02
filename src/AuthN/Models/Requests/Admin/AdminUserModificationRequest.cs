using System.Collections.Generic;
using AuthN.Models.Requests.User;
using AuthN.Models.User;

namespace AuthN.Models.Requests.Admin {
    public class AdminUserModificationRequest : UserModificationRequest {
        public bool enabled { get; set; } = true;
        public List<string> groups { get; set; } = new List<string>();

        public override void apply(UserIdentity user) {
            base.apply(user);

            user.enabled = enabled;
            user.groups = groups;
        }
    }
}