using HotChocolate;

namespace GraphQL.Schema
{
    public class Query
    {
        // Resolver that returns a single Employee object
        [GraphQLName("getEmployee")]
        public Employee GetEmployee() =>
            new Employee { Id = 1, Name = "Alice", Age = 30 };

        // Resolver that returns a list of Employees
        [GraphQLName("getEmployees")]
        public List<Employee> GetEmployees() =>
            new List<Employee>
            {
                new Employee { Id = 1, Name = "Alice", Age = 30 },
                new Employee { Id = 2, Name = "Bob", Age = 25 },
                new Employee { Id = 3, Name = "Charlie", Age = 40 }
            };

        [GraphQLName("employeeById")]
        public Employee GetEmployeeById(int id)
        {
            var employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "Alice", Age = 30 },
                new Employee { Id = 2, Name = "Bob", Age = 25 },
                new Employee { Id = 3, Name = "Charlie", Age = 40 }
            };

            return employees.FirstOrDefault(e => e.Id == id)!;
        }
    }
}


public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Age { get; set; }
}