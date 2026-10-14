namespace CleanOrders.Application
{


    public class ConfirmOrderUseCase
    {
        private readonly IOrderRepository _repository;

        public ConfirmOrderUseCase(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);

            if (order is null)
                return false;

            order.Confirm();

            await _repository.SaveAsync(order);

            return true;
        }
    }
}