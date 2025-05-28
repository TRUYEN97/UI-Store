using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiStore.Models
{
    public class AccountModel
    {
        public string Id { get; private set; }
        public string Password { get; private set; }
        public AccountModel(string id, string pw) 
        {
            Id = id;
            Password = pw;
        }
    }
}
