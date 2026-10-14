using CleanOrders.Application;
using CleanOrders.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanOrders.Infrastructure
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<int, Order> _orders = new()
        {
            [1] = new Order(1, 100)
        };

        public Task<Order?> GetByIdAsync(int id)
        {
            _orders.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task SaveAsync(Order order)
        {
            _orders[order.Id] = order;
            return Task.CompletedTask;
        }
    }
}
