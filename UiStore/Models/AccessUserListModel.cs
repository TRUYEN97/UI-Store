using System.Collections.Generic;

namespace UiStore.Models
{
    internal class AccessUserListModel
    {
        public HashSet<UserModel> UserModels { get; set; } = new HashSet<UserModel>();
    }
}
