using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CityEventsAPI.Data;
using CityEventsAPI.Models;
using CityEventsAPI.ViewModels;

namespace CityEventsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketOrdersController : ControllerBase
    {
        private readonly EventContext _context;

        public TicketOrdersController(EventContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получение списка всех заказов на билеты
        /// </summary>
        /// <returns>Список заказов в формате JSON</returns>
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<TicketOrderViewModel>>> GetAllTicketOrders()
        {
            var ticketOrders = await _context.TicketOrders
                .Include(o => o.Event)
                .Include(o => o.Customer)
                .Select(o => new TicketOrderViewModel
                {
                    OrderID = o.OrderID,
                    EventName = o.Event.EventName,
                    CustomerName = o.Customer.FullName,
                    OrderDate = o.OrderDate,
                    TicketCount = o.TicketCount
                }).ToListAsync();

            return Ok(ticketOrders);
        }

        /// <summary>
        /// Получение данных одного заказа
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <returns>Информация о заказе</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketOrderViewModel>> GetTicketOrder(int id)
        {
            var ticketOrder = await _context.TicketOrders
                .Include(o => o.Event)
                .Include(o => o.Customer)
                .Where(o => o.OrderID == id)
                .Select(o => new TicketOrderViewModel
                {
                    OrderID = o.OrderID,
                    EventID = o.Event.EventID,
                    CustomerID = o.Customer.CustomerID,
                    EventName = o.Event.EventName,
                    CustomerName = o.Customer.FullName,
                    OrderDate = o.OrderDate,
                    TicketCount = o.TicketCount
                }).FirstOrDefaultAsync();

            if (ticketOrder == null)
                return NotFound();

            return Ok(ticketOrder);
        }

        /// <summary>
        /// Получение списка заказов по названию мероприятия
        /// </summary>
        /// <param name="eventName">Название мероприятия</param>
        /// <returns>Список заказов, соответствующих названию мероприятия</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TicketOrderViewModel>>> GetTicketOrdersByEventName([FromQuery] string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return BadRequest("Event name is required.");
            }

            var ticketOrders = await _context.TicketOrders
                .Include(o => o.Event)
                .Include(o => o.Customer)
                .Where(o => o.Event.EventName.Contains(eventName))
                .Select(o => new TicketOrderViewModel
                {
                    OrderID = o.OrderID,
                    EventName = o.Event.EventName,
                    CustomerName = o.Customer.FullName,
                    OrderDate = o.OrderDate,
                    TicketCount = o.TicketCount
                }).ToListAsync();

            return Ok(ticketOrders);
        }

        /// <summary>
        /// Создание нового заказа на билеты
        /// </summary>
        /// <param name="ticketOrder">Данные нового заказа</param>
        /// <returns>Статус создания</returns>
        [HttpPost]
        public async Task<ActionResult<TicketOrder>> CreateTicketOrder([FromBody] TicketOrder ticketOrder)
        {
            if (ticketOrder == null)
                return BadRequest("Invalid data.");

            _context.TicketOrders.Add(ticketOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketOrder), new { id = ticketOrder.OrderID }, ticketOrder);
        }

        /// <summary>
        /// Обновление информации о заказе на билеты
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <param name="ticketOrder">Обновленные данные заказа</param>
        /// <returns>Статус обновления</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketOrder(int id, [FromBody] TicketOrder ticketOrder)
        {
            if (id != ticketOrder.OrderID)
                return BadRequest("Mismatched order ID.");

            if (!_context.TicketOrders.Any(o => o.OrderID == id))
                return NotFound();

            _context.Entry(ticketOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Удаление заказа на билеты
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <returns>Статус операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketOrder(int id)
        {
            var ticketOrder = await _context.TicketOrders.FindAsync(id);
            if (ticketOrder == null)
                return NotFound();

            _context.TicketOrders.Remove(ticketOrder);
            await _context.SaveChangesAsync();

            return Ok(ticketOrder);
        }

        /// <summary>
        /// Получение списка всех клиентов
        /// </summary>
        /// <returns>Список клиентов с их идентификаторами и полными именами</returns>
        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<CustomerViewModel>>> GetCustomers()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    FullName = c.FullName
                }).ToListAsync();

            return Ok(customers);
        }

        /// <summary>
        /// Получение списка мероприятий
        /// </summary>
        /// <returns>Список мероприятий, состоящий из первых и последних записей, объединенных в один список</returns>
        [HttpGet("events")]
        public async Task<ActionResult<IEnumerable<EventViewModel>>> GetEvents(int firstLimit = 5000, int lastLimit = 100)
        {
            var firstRecords = await _context.Events
                .Take(firstLimit)
                .Select(e => new EventViewModel
                {
                    EventID = e.EventID,
                    EventName = e.EventName
                }).ToListAsync();

            var lastRecords = await _context.Events
                .OrderByDescending(e => e.EventID)
                .Take(lastLimit)
                .Select(e => new EventViewModel
                {
                    EventID = e.EventID,
                    EventName = e.EventName
                }).ToListAsync();

            var combinedRecords = firstRecords.Concat(lastRecords).ToList();

            return Ok(combinedRecords);
        }
    }
}