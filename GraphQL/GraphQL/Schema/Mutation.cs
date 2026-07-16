using HotChocolate;
using HotChocolate.Subscriptions;

namespace GraphQL.Schema
{
    public class Mutation
    {
        private static readonly List<Employee> _employees = new()
        {
            new Employee { Id = 1, Name = "Alice", Age = 30 },
            new Employee { Id = 2, Name = "Bob", Age = 25 }
        };

   

        // Update
        [GraphQLName("updateEmployee")]
        public Employee UpdateEmployee(int id, string name, int age)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee is null) throw new Exception("Employee not found");

            employee.Name = name;
            employee.Age = age;
            return employee;
        }

        // Delete
        [GraphQLName("deleteEmployee")]
        public bool DeleteEmployee(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee is null) return false;

            _employees.Remove(employee);
            return true;
        }
        public class EmployeeInput
        {
            public string Name { get; set; } = default!;
            public int Age { get; set; }
        }
        [GraphQLName("createEmployee")]
        public async Task<Employee> CreateEmployee(
            string name, int age, [Service] ITopicEventSender sender)
        {
            var newEmployee = new Employee { Id = _employees.Count + 1, Name = name, Age = age };
            _employees.Add(newEmployee);

            // Publish event for subscriptions
            await sender.SendAsync(nameof(Subscription.OnEmployeeCreated), newEmployee);

            return newEmployee;
        }
    }
   
    }
