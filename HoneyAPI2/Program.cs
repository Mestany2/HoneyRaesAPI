using HoneyAPI2.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
List<Customer> customers = new List<Customer> { };
List<Employee> employees = new List<Employee> { };
List<ServiceTicket> serviceTickets = new List<ServiceTicket> { };

//Create lists
List<Customer> customerList = new List<Customer> ()
{
    new Customer { 
        Id = 1,
        Name = "Sterling",
        Address = "123 Street"
},
    new Customer { 
        Id = 2,
        Name = "Foden",
        Address = "456 Road"
},
    new Customer {
        Id = 3,
        Name = "Henderson",
        Address = "789 Circle"
}
};

List<Employee> employeeList = new List<Employee>()
{
    new Employee
    {
        Id = 4,
        Name = "Jack",
        Specialty = "Food"
    },   
    new Employee
    {
        Id = 5,
        Name = "Lauren",
        Specialty = "Manager"
    },

};

List<ServiceTicket> serviceTicketsList = new List<ServiceTicket>()
{
    new ServiceTicket
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 4,
        Description = "Office Party",
        Emergency = false,
        DateCompleted = "2023-07-01"
    },    
    new ServiceTicket
    {
        Id = 6,
        CustomerId = 1,
        EmployeeId = 5,
        Description = "Budget Review",
        Emergency = false,
        DateCompleted = "2023-07-05"
    },    
    new ServiceTicket
    {
        Id = 7,
        CustomerId = 3,
        Description = "Cleaning",
        Emergency = true,
    },    
    new ServiceTicket
    {
        Id = 8,
        CustomerId = 2,
        EmployeeId = 5,
        Description = "Stock",
        Emergency = true,
    },   
    new ServiceTicket
    {
        Id = 9,
        CustomerId = 3,
        Description = "Mail",
        Emergency = false,

    },

};
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Service Tickets Endpoints

app.MapGet("/servicetickets", () =>
{
    return serviceTicketsList;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTicketsList.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employeeList.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customerList.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTicketsList.Max(st => st.Id) + 1;
    serviceTicketsList.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTicketsList.FirstOrDefault(st => st.Id == id);
    serviceTicketsList.Remove(serviceTicket);
});
//Employees Endpoints

app.MapGet("/employees", () =>
{
    return employeeList;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employeeList.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTicketsList.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);

});

//Customer Endpoint

app.MapGet("/customers", () =>
{
    return customerList;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customerList.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTicketsList.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});


app.Run();
