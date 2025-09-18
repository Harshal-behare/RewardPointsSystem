using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IUserService
    {
        void AddUser(User user);
        User GetUserByEmail(string email);
        IEnumerable<User> GetAllUsers();
    }

    public interface IProductService
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(Guid id);
    }

    public interface IEventService
    {
        void CreateEvent(Event evnt);
        IEnumerable<Event> GetAllEvents();
    }

    public interface IRedemptionService
    {
        Redemption RedeemProduct(User user, Product product);
    }
}

