using LAB3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LAB3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            List<Client> clients = _dbContext.Clients.ToList();
            return View(clients);
        }

        public IActionResult Create()
        {
            return View(new Client());
        }

        [HttpPost]
        public IActionResult Create(Client client)
        {
            _dbContext.Clients.Add(client);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Update(int Id)
        {
            Client client = _dbContext.Clients.SingleOrDefault(x => x.Id == Id);
            return View(client);
        }

        [HttpPost]
        public IActionResult Update(Client client)
        {
            _dbContext.Clients.Update(client);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(Client client)
        {
            _dbContext.Clients.Remove(client);
            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
