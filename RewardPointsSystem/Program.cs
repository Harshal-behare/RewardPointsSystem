using RewardPointsSystem.Models;
using RewardPointsSystem.Services;

class Program
{
    static void Main(string[] args)
    {
        var userService = new UserService();
        var productService = new ProductService();
        var transactionService = new PointsTransactionService();
        var redemptionService = new RedemptionService(transactionService);

        
        var user = new User("Harshal Behare", "harshal.behare@agdata.com", "EMP001");
        userService.AddUser(user);

        
        var product = new Product { Name = "Coffee Mug", RequiredPoints = 50, Stock = 10 };
        productService.AddProduct(product);

      
        user.AddPoints(100);
        transactionService.AddTransaction(user, 100, "Earn");
        Console.WriteLine($"{user.Name} Balance: {user.PointsBalance}");

        
        var redemption = redemptionService.RedeemProduct(user, product);
        Console.WriteLine($"{user.Name} redeemed {redemption.Product.Name} | New Balance: {user.PointsBalance}");

        
        Console.WriteLine("\n--- Transaction History ---");
        foreach (var tx in transactionService.GetAllTransactions())
        {
            Console.WriteLine($"{tx.Timestamp}: {tx.User.Name} {tx.Type} {tx.Points} points");
        }
    }
}
