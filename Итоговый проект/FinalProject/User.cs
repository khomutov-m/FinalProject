using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public class User
    {
        public int UserId { get; set; }
        public string UserSurname { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserPatronymic { get; set; } = "";
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public int UserRole { get; set; }
        public string UserFullName { get => UserSurname +" "+ UserName +" "+ UserPatronymic; set { } }
    }
}
