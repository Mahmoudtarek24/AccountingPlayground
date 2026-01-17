using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.Identity.Client;

namespace AccountingPlayground.Application.Dto_s
{
    public class AccountDto
    {
        public int Id { get; set; }  
        public string Code { get; set; }  
        public string Name { get; set; }  
        public AccountType Type { get; set; }  
    }
}
