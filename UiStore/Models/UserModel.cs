using System;

namespace UiStore.Models
{
    internal class UserModel
    {
        private string _id;
        private string _password;

        public string Id { get => _id; set => _id = value; }
        public string Password { get => _password; set => _password = value; }
        public override bool Equals(object obj)
        {
            if (obj is UserModel other)
            {
                return string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase);
            }
            else if(obj is string id)
            {
                return string.Equals(this.Id, id, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this._id?.ToLower().GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _id ?? "";
        }
    }
}
