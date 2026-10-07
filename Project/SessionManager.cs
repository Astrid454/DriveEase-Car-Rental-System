using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Backend.Models;

namespace Project
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool isAdmin => IsLoggedIn && CurrentUser.Role == "Admin";
    }
}
