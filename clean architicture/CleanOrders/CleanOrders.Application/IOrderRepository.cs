using CleanOrders.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanOrders.Application
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task SaveAsync(Order order);
    }
}
