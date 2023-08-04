using HoneyAPI2.Models;
using System.Linq;

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
    new Employee
    {
        Id = 20,
        Name = "Smith",
        Specialty = "Worker"
    },

};

List<ServiceTicket> serviceTicketsList = new List<ServiceTicket>()
{
    new ServiceTicket
    {
        Id = 5,
        CustomerId = 2,
        Description = "Office Party",
        Emergency = true,
        
    },    
    new ServiceTicket
    {
        Id = 6,
        CustomerId = 1,
        EmployeeId = 4,
        Description = "Budget Review",
        Emergency = false,
        DateCompleted = new DateTime(2021,07,21)
    },    
    new ServiceTicket
    {
        Id = 7,
        CustomerId = 3,
        EmployeeId = 5,
        Description = "Cleaning",
        Emergency = true,
        DateCompleted = new DateTime(2023,07,21)
    },    
    new ServiceTicket
    {
        Id = 8,
        CustomerId = 2,
        EmployeeId = 5,
        Description = "Stock",
        Emergency = true,
        DateCompleted = new DateTime(2023,07,24)
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

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTicketsList.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTicketsList.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTicketsList[ticketIndex] = serviceTicket;
    return Results.Ok();
});

//Past Ticket Review

app.MapGet("/servicetickets/review", () =>
{
    List<ServiceTicket> pastreview = serviceTicketsList.OrderBy(d => d.DateCompleted).ToList();
    return pastreview;
}
);

//Prioritized Tickets

app.MapGet("/servicetickets/priority", () =>
{
    List<ServiceTicket> Priorityticket = serviceTicketsList.Where(st => st.DateCompleted == new DateTime()).OrderByDescending(st => st.Emergency).ThenByDescending(st => st.EmployeeId).ToList();
    return Priorityticket;
}
);

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTicketsList.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/servicetickets/emergencies", () =>
{
    List<ServiceTicket> emergencies = serviceTicketsList.Where(st => st.Emergency == true && st.DateCompleted == new DateTime()).ToList();
    return emergencies;
});

app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassigned = serviceTicketsList.Where(st => st.EmployeeId == null).ToList();
    return unassigned;
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

app.MapGet("/employees/available", () =>
{
    List<Employee> available = employeeList.Where(e => !serviceTicketsList.Any(st => st.EmployeeId == e.Id)).ToList();
    return available;

});

app.MapGet("/employees/employeeofthemonth", () =>
{
    var employeeOfTheMonth = employeeList.OrderByDescending(e => serviceTicketsList.Count(st => st.EmployeeId == e.Id && st.DateCompleted >= DateTime.Now.AddMonths(-1))).FirstOrDefault(); ;
    return employeeOfTheMonth;
}
);

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

app.MapGet("customers/inactive", () =>
{
    List<int> activeIds = serviceTicketsList.Where(t => t.DateCompleted >= DateTime.Today.AddYears(-1)).Select(ticket => ticket.CustomerId).ToList();
    List<Customer> inactive = customerList.Where(c => !activeIds.Contains(c.Id)).ToList();
    return inactive;
});


app.MapGet("/customers/byemployee", (int id) =>
{
    List<Customer> employeeCustomer = customerList.Where( c => serviceTicketsList.Any(st => st.CustomerId == c.Id && st.EmployeeId == id)).ToList();
    return employeeCustomer;

});



app.Run();
