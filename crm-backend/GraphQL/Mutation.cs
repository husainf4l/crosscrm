using HotChocolate;
using crm_backend.Modules.User;
using crm_backend.Modules.Company;
using crm_backend.Modules.Customer;

namespace crm_backend.GraphQL;

public class Mutation
{
    public string HelloMutation() => "Hello from CRM Backend Mutations";
}