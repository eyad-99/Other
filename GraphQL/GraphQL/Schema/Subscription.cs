using HotChocolate;
using HotChocolate.Types;

namespace GraphQL.Schema
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public Employee OnEmployeeCreated([EventMessage] Employee employee) => employee;
    }
}
