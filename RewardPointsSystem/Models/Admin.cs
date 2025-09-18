using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class Admin : User
    {
        public Admin(string name, string email, string employeeId)
            : base(name, email, employeeId) { }
    }
}

