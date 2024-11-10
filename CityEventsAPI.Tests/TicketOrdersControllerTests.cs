using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CityEventsAPI.Controllers;
using CityEventsAPI.Data;
using CityEventsAPI.Models;
using CityEventsAPI.ViewModels;

namespace CityEventsAPI.Tests
{
    public class TicketOrdersControllerTests
    {
        private readonly EventContext _context;
        private readonly TicketOrdersController _controller;

        public TicketOrdersControllerTests()
        {
            var options = new DbContextOptionsBuilder<EventContext>()
                .UseInMemoryDatabase(databaseName: "CityEventsTestDatabase")
                .Options;
            _context = new EventContext(options);

            _controller = new TicketOrdersController(_context);
            ClearDatabase();
            SeedDatabase();
        }

        private void ClearDatabase()
        {
            _context.TicketOrders.RemoveRange(_context.TicketOrders);
            _context.Events.RemoveRange(_context.Events);
            _context.Customers.RemoveRange(_context.Customers);
            _context.SaveChanges();
        }

        private void SeedDatabase()
        {
            var event1 = new Event { EventID = 1, EventName = "Concert" };
            var event2 = new Event { EventID = 2, EventName = "Theater" };
            var customer1 = new Customer { CustomerID = 1, FullName = "John Doe" };
            var customer2 = new Customer { CustomerID = 2, FullName = "Jane Smith" };

            var ticketOrder1 = new TicketOrder { OrderID = 1, Event = event1, Customer = customer1, TicketCount = 2 };
            var ticketOrder2 = new TicketOrder { OrderID = 2, Event = event2, Customer = customer2, TicketCount = 3 };

            _context.Events.AddRange(event1, event2);
            _context.Customers.AddRange(customer1, customer2);
            _context.TicketOrders.AddRange(ticketOrder1, ticketOrder2);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllTicketOrders_ReturnsList()
        {
            var result = await _controller.GetAllTicketOrders();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orders = Assert.IsType<List<TicketOrderViewModel>>(okResult.Value);
            Assert.Equal(2, orders.Count);
        }

        [Fact]
        public async Task GetTicketOrder_ReturnsTicketOrder()
        {
            var result = await _controller.GetTicketOrder(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var order = Assert.IsType<TicketOrderViewModel>(okResult.Value);
            Assert.Equal(1, order.OrderID);
        }

        [Fact]
        public async Task GetTicketOrder_ReturnsNotFound()
        {
            var result = await _controller.GetTicketOrder(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTicketOrdersByEventName_ReturnsOrders()
        {
            var result = await _controller.GetTicketOrdersByEventName("Concert");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orders = Assert.IsType<List<TicketOrderViewModel>>(okResult.Value);
            Assert.Single(orders);
        }

        [Fact]
        public async Task GetTicketOrdersByEventName_ReturnsEmptyList()
        {
            var result = await _controller.GetTicketOrdersByEventName("NonExistentEvent");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var orders = Assert.IsType<List<TicketOrderViewModel>>(okResult.Value);
            Assert.Empty(orders);
        }

        [Fact]
        public async Task CreateTicketOrder_ReturnsCreatedOrder()
        {
            var newOrder = new TicketOrder { OrderID = 3, EventID = 1, CustomerID = 2, TicketCount = 1 };
            var result = await _controller.CreateTicketOrder(newOrder);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdOrder = Assert.IsType<TicketOrder>(createdResult.Value);
            Assert.Equal(3, createdOrder.OrderID);
        }

        [Fact]
        public async Task CreateTicketOrder_ReturnsBadRequest()
        {
            var result = await _controller.CreateTicketOrder(null);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid data.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateTicketOrder_ReturnsBadRequest()
        {
            var orderToUpdate = new TicketOrder { OrderID = 1, EventID = 1, CustomerID = 1, TicketCount = 5 };
            var result = await _controller.UpdateTicketOrder(2, orderToUpdate);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Mismatched order ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateTicketOrder_ReturnsNotFound()
        {
            var orderToUpdate = new TicketOrder { OrderID = 999, EventID = 1, CustomerID = 1, TicketCount = 5 };
            var result = await _controller.UpdateTicketOrder(999, orderToUpdate);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTicketOrder_ReturnsDeletedOrder()
        {
            var result = await _controller.DeleteTicketOrder(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var deletedOrder = Assert.IsType<TicketOrder>(okResult.Value);
            Assert.Equal(1, deletedOrder.OrderID);
        }

        [Fact]
        public async Task DeleteTicketOrder_ReturnsNotFound()
        {
            var result = await _controller.DeleteTicketOrder(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCustomers_ReturnsListOfCustomers()
        {
            var result = await _controller.GetCustomers();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var customers = Assert.IsType<List<CustomerViewModel>>(okResult.Value);
            Assert.Equal(2, customers.Count);
        }

        [Fact]
        public async Task GetEvents_ReturnsListOfEvents()
        {
            var result = await _controller.GetEvents();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsType<List<EventViewModel>>(okResult.Value);
            Assert.Equal(4, events.Count);
        }
    }
}