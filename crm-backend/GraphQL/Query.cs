using HotChocolate;
using crm_backend.Modules.User;
using crm_backend.Modules.Company;
using crm_backend.Modules.Customer;

namespace crm_backend.GraphQL;

public class Query
{
    public string Hello() => "World from CRM Backend";
}