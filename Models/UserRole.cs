﻿using System.Data;
using ToDoApp.Domain;

namespace ToDoApp.Models
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
   

}
