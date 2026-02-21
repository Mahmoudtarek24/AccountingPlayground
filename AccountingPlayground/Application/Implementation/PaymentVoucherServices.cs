using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Application.Results;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Entities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace AccountingPlayground.Application.Implementation
{
    public class PaymentVoucherServices : IPaymentVoucherServices
    {
        //private IFinancialAccountRepository financialAccountRepository {  get; }

        private readonly IFinancialAccountRepository financialAccountRepository;
        private readonly IJournalEntryRepository journalEntryRepository;
        private readonly IJournalEntryService journalEntryService;
        private readonly ISupplierRepository supplierRepository;
        private readonly ApplicationDbContext context;
        public PaymentVoucherServices(IFinancialAccountRepository financialAccountRepository,
                                      IJournalEntryRepository journalEntryRepository,
                                      IJournalEntryService journalEntryService,
                                      ISupplierRepository supplierRepository,
                                      ApplicationDbContext context)
        {
            this.context = context;
            this.financialAccountRepository = financialAccountRepository;
            this.journalEntryService = journalEntryService;
            this.journalEntryRepository = journalEntryRepository;
            this.supplierRepository = supplierRepository;
        }
        // need to know when have transaction inside this method and also journalEntry have transaction
        public async Task<bool> CreatePaymentVoucher(CreatePaymentVoucherDto dto)
        {
            if (dto.SettlementType == SettlementType.PayableSettlement)
            {

            }


            if (dto.PaymentMethod == Dto_s.PaymentMethod.Cash)
            {
                if (dto.CashSessionId is null)
                    return false;

                //var session = await context.CashSessions.FindAsync(dto.CashSessionId);
                //if (session is null || !session.i)
                //    return Result.Fail<int>("Cash session is not open.");
            }

            // check if employee exist 
            var employee = await context.Employees.FindAsync(dto.EmployeeId);
            if (employee is null)
                return false;

            var accountIds = new List<int> { dto.VatAccountId, dto.CashAccountId, dto.ExpenseAccountId };
            var ValidAccount = await financialAccountRepository.GetValidAnyAccountTypeIdsAsync(accountIds);

            if (ValidAccount.Count != accountIds.Count)
                return false;


            await ValidateAccountForRole(dto.ExpenseAccountId, AccountRole.Expense);
            await ValidateAccountForRole(dto.VatAccountId, AccountRole.VatRecoverable);
            await ValidateAccountForRole(dto.CashAccountId, AccountRole.Cash);


            var paymentVoucher = new PaymentVoucher()
            {
                VoucherDate = dto.VoucherDate,
                EmployeeId = dto.EmployeeId,
                NetAmount = (long)dto.NetAmount,
                VatAmount = (long)dto.VATAmount,
                TotalAmount = (long)(dto.NetAmount + dto.VATAmount),
                CashSessionId = dto.CashSessionId,
                PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                //  VoucherNo = dto. need see any role used on this point generate it from data base or create algorizme to it
                VoucherNo = await GenerateVoucherNumber()
            };

            // i will build foreach replace this code 
            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.ExpenseAccountId,
                Amount = (long)dto.NetAmount,
            });
            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.VatAccountId,
                Amount = (long)dto.VATAmount,
            });


            var journalEntry = new JournalEntryPostModel()
            {
                //Reference = what should write her 
                Reference = $"PV-{paymentVoucher.VoucherNo}", // and her need to more clarification for  reference 
                EntryDate = dto.VoucherDate,
            };
            // will also replace this code with foreach 

            journalEntry.Lines = new List<JournalEntryLinePostModel>
            {
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.VatAccountId,
                    Debit = (long)dto.VATAmount,
                    Credit =0
                },
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.CashAccountId,
                    Debit = 0,
                    Credit =(long)(dto.VATAmount+dto.NetAmount)
                },
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.ExpenseAccountId,
                    Debit = (long)dto.NetAmount,
                    Credit =0
                },
            };
            var result = await journalEntryService.PostJournalEntry(journalEntry);
            if (result != JournalEntryError.CreatedSuccessfully)
                return false;

            await context.PaymentVouchers.AddAsync(paymentVoucher);
            await context.SaveChangesAsync();

            return true;
        }
        private async Task<string> GenerateVoucherNumber() //will prevent race condition her
        {
            var count = await context.PaymentVouchers.CountAsync() + 1;
            return $"PV-{count:D5}";
        }

        public bool Validation(CreatePaymentVoucherDto dto)
        {
            if (dto.NetAmount <= 0) return false;
            if (dto.VATAmount < 0) return false;
            if (dto.NetAmount + dto.VATAmount <= 0) return false;

            return true;
        }

        public async Task ValidateAccountForRole(int accountId, AccountRole role)
        {
            var account = await context.FinancialAccounts.Where(a => a.Id == accountId)
                             .Select(a => new { a.IsLeaf, a.Type }).FirstOrDefaultAsync();

            if (account == null)
                throw new Exception("Account not found");

            if (!account.IsLeaf)
                throw new Exception("Account must be a leaf");

            var isValid = role switch
            {
                AccountRole.Expense => account.Type == AccountType.Expense,
                AccountRole.VatRecoverable => account.Type == AccountType.Asset,
                AccountRole.Cash => account.Type == AccountType.Asset,
                AccountRole.Bank => account.Type == AccountType.Asset,
                AccountRole.Payable => account.Type == AccountType.Liability,
                _ => false
            };

            if (!isValid)
                throw new Exception($"Account not valid for role {role}");
        }

        //MultipleExpensesInOnePayment
        //Invoice + Full Payment (Settlement / Closing Liability)
        //  Invoice + Partial Payment
        //Balance = Total Credits - Total Debits
        public async Task<bool> PayableSettlement2(CreatePaymentVoucherDto dto)
        {
            //if (dto.BankAccountId is null && dto.CashAccountId is null)   , need to set on parent method 
            //    return false;

            if (dto.PayableAccountId is null)
                return false;

            if (dto.PaidAmount <= 0)
                return false;

            var paidAccount = dto.BankAccountId ?? dto.CashAccountId; //  will set this line of the first of class factory 

            var accountIds = new List<int> { paidAccount, dto.PayableAccountId!.Value };
            var ValidAccount = await financialAccountRepository.GetValidAnyAccountTypeIdsAsync(accountIds);

            if (ValidAccount.Count != accountIds.Count)
                return false;

            await ValidateAccountForRole(dto.PayableAccountId.Value, AccountRole.Payable);
            if (dto.BankAccountId is not null)
                await ValidateAccountForRole(dto.BankAccountId.Value, AccountRole.Bank);
            else
                await ValidateAccountForRole(dto.CashAccountId, AccountRole.Cash);


            var journalEntryForPayableAccount = await journalEntryRepository.GetJournalEntryLinesOfAccount(dto.PayableAccountId.Value);


            long OpenBalance = 0;  // this is money need to pay it 


            foreach (var journalEnty in journalEntryForPayableAccount)
                OpenBalance += (journalEnty.Credit - journalEnty.Debit);


            if (dto.PaidAmount == OpenBalance)
            {

            }



            if (dto.PaidAmount > OpenBalance)
            {
                // overpayment 
            }


            var paymentVoucher = new PaymentVoucher()
            {
                VoucherDate = dto.VoucherDate,
                EmployeeId = dto.EmployeeId,
                CashSessionId = dto.CashSessionId,
                PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                TotalAmount = (long)dto.PaidAmount,
                //  VoucherNo = dto. need see any role used on this point generate it from data base or create algorizme to it
                VoucherNo = await GenerateVoucherNumber()
            };

            // i will build foreach replace this code 
            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.PayableAccountId!.Value,
                Amount = (long)dto.PaidAmount,
            });







            var journalEntry = new JournalEntryPostModel()
            {
                //Reference = what should write her 
                Reference = $"PV-{paymentVoucher.VoucherNo}", // and her need to more clarification for  reference 
                EntryDate = dto.VoucherDate,
            };
            // will also replace this code with foreach 

            journalEntry.Lines = new List<JournalEntryLinePostModel>
            {
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.PayableAccountId.Value,
                    Debit = (long)dto.PaidAmount,
                    Credit =0
                },
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.BankAccountId??dto.CashAccountId,
                    Debit = 0,
                    Credit = (long)dto.PaidAmount,
                },

            };
            var result = await journalEntryService.PostJournalEntry(journalEntry);
            if (result != JournalEntryError.CreatedSuccessfully)
                return false;

            await context.PaymentVouchers.AddAsync(paymentVoucher);
            await context.SaveChangesAsync();

            return true;


        }

        public async Task<bool> CreatePaymentVoucherWithholding(CreatePaymentVoucherDto dto)
        {
            if (dto.PayableAccountId is null)
                return false;

            var supplier = await supplierRepository.GetSupplierById(dto.PayableAccountId.Value);

            if (dto.PaidAmount <= 300)
            {
                // complete without Withholding // will going to normal scenario
            }
            if (!supplier.IsSubjectToWithholding)
            {
                //  complete without Withholding , Normal scenario 
            }
            var rate = PercentageWithholding(supplier);
            var WithheldAmount = (decimal)rate * dto.PaidAmount;
            var NetPaidToSupplier = dto.PaidAmount - WithheldAmount;

            // need to transfer on repository
            var withholdingAccount = await context.FinancialAccounts
                        .FirstAsync(a => a.SystemRole == SystemAccountType.WithholdingTaxPayable);
            ///


            var paymentVoucher = new PaymentVoucher()
            {
                VoucherDate = dto.VoucherDate,
                EmployeeId = dto.EmployeeId,
                NetAmount = 0,
                VatAmount = 0,
                TotalAmount = (long)dto.PaidAmount,
                CashSessionId = dto.CashSessionId,
                PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                //  VoucherNo = dto. need see any role used on this point generate it from data base or create algorizme to it
                VoucherNo = await GenerateVoucherNumber()
            };

            // i will build foreach replace this code 
            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.PayableAccountId.Value,
                Amount = (long)NetPaidToSupplier,
                // Description = "Supplier Gross Amount"
            });

            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = withholdingAccount.Id,
                Amount = (long)WithheldAmount,
                // Description = "Withholding Tax"
            });

            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.CashAccountId, // or Bank
                Amount = (long)NetPaidToSupplier,
                // Description = "Net Paid to Supplier"
            });


            var journalEntry = new JournalEntryPostModel()
            {
                //Reference = what should write her 
                Reference = $"PV-{paymentVoucher.VoucherNo}", // and her need to more clarification for  reference 
                EntryDate = dto.VoucherDate,
            };
            // will also replace this code with foreach 

            journalEntry.Lines = new List<JournalEntryLinePostModel>
            {
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.PayableAccountId.Value,
                    Debit = (long)dto.PaidAmount,
                    Credit =0
                },
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = dto.CashAccountId, // or bank account 
                    Debit = 0,
                    Credit = (long)NetPaidToSupplier
                },
                new JournalEntryLinePostModel()
                {
                    FinancialAccountId = withholdingAccount.Id,
                    Debit = 0,
                    Credit =(long)WithheldAmount
                },
            };
            var result = await journalEntryService.PostJournalEntry(journalEntry);
            if (result != JournalEntryError.CreatedSuccessfully)
                return false;

            await context.PaymentVouchers.AddAsync(paymentVoucher);
            await context.SaveChangesAsync();


            return default;
        }
        public double PercentageWithholding(Supplier supplier)
        {
            return supplier switch
            {
                { TaxRegistrationStatus: TaxRegistrationStatus.Exempt } => 0.0,
                { TaxRegistrationStatus: TaxRegistrationStatus.Registered, IsSubjectToWithholding: true } => 0.01,
                { TaxRegistrationStatus: TaxRegistrationStatus.NotRegistered, IsSubjectToWithholding: true } => 0.05,
                _ => 0.0,
            };
        }


        public async Task<bool> CreatePaymentVoucherWithholding2(CreatePaymentVoucherDto dto)
        {
            // need to check the supplier related to PayableAccountId 
            if (dto.PayableAccountId is null)
                return false;

            if (dto.GrossAmount is null || dto.GrossAmount <= 0)
                return false;

            var grossAmount = dto.GrossAmount.Value;

            // 1️⃣ Get Supplier
            var supplier = await supplierRepository.GetSupplierById(dto.PayableAccountId.Value);
            if (supplier is null)
                return false;

            // 2️⃣ Resolve paid-from account
            var paidFromAccountId = dto.PaymentMethod == Dto_s.PaymentMethod.Cash
                ? dto.CashAccountId
                : dto.BankAccountId;

            if (paidFromAccountId is null)
                return false;

            // 3️⃣ Gate: below threshold OR not subject → normal settlement
            if (grossAmount <= 300 || !supplier.IsSubjectToWithholding)
            {
                dto.PaidAmount = grossAmount; // delegate using existing flow
                return await PayableSettlement2(dto);
            }

            // 4️⃣ Determine withholding rate
            var rate = PercentageWithholding(supplier);
            if (rate <= 0)
            {
                dto.PaidAmount = grossAmount;
                return await PayableSettlement2(dto);
            }

            var withheldAmount = grossAmount * (decimal)rate;
            var netPaid = grossAmount - withheldAmount;

            // 5️⃣ Resolve system accounts
            //var payableAccount = await context.FinancialAccounts
            //    .FirstAsync(a => a.SystemRole == SystemAccountType.AccountsPayable);

            var withholdingAccount = await context.FinancialAccounts
                .FirstAsync(a => a.SystemRole == SystemAccountType.WithholdingTaxPayable);

            using var tx = await context.Database.BeginTransactionAsync();

            try
            {
                // 6️⃣ Create Payment Voucher (Document)
                var paymentVoucher = new PaymentVoucher
                {
                    VoucherDate = dto.VoucherDate,
                    EmployeeId = dto.EmployeeId,
                    PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                    CashSessionId = dto.CashSessionId,
                    TotalAmount = (long)grossAmount,
                    VoucherNo = await GenerateVoucherNumber()
                };

                paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = dto.PayableAccountId.Value,
                    Amount = (long)grossAmount
                });

                paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = withholdingAccount.Id,
                    Amount = (long)withheldAmount
                });

                paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = paidFromAccountId.Value,
                    Amount = (long)netPaid
                });

                // 7️⃣ Journal Entry (Accounting Truth)
                var journalEntry = new JournalEntryPostModel
                {
                    EntryDate = dto.VoucherDate,
                    Reference = $"Withholding Payment - Supplier #{supplier.Id}"
                };

                journalEntry.Lines = new List<JournalEntryLinePostModel>
                {
                    new()
                    {
                        FinancialAccountId = dto.PayableAccountId.Value,
                        Debit = (long)grossAmount
                    },
                    new()
                    {
                        FinancialAccountId = paidFromAccountId.Value,
                        Credit = (long)netPaid
                    },
                    new()
                    {
                        FinancialAccountId = withholdingAccount.Id,
                        Credit = (long)withheldAmount
                    }
                };

                var result = await journalEntryService.PostJournalEntry(journalEntry);
                if (result != JournalEntryError.CreatedSuccessfully)
                    return false;

                await context.PaymentVouchers.AddAsync(paymentVoucher);
                await context.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        // will be step inside PayableSettlement2
        public async Task<bool> CreatePaymentVoucherWithOverpayment(CreatePaymentVoucherDto dto)
        {
            // need validation
            // need to get money value should pay to this supplier 

            //PayableAccountIdOverpayment

            var journalEntryForPayableAccountId = await context.JournalEntryLines
                .Where(e => e.FinancialAccountId == dto.PayableAccountIdOverpayment && !e.JournalEntry.FinancialYear.IsClosed)
                .Select(e => new { e.Debit, e.Credit }).ToListAsync();

            var OpenBalance = journalEntryForPayableAccountId.Sum(e => e.Credit) - journalEntryForPayableAccountId.Sum(e => e.Debit);


            if (dto.PaidAmountOverpayment > OpenBalance)
            {
                var OverAmount = dto.PaidAmountOverpayment - OpenBalance;



                var paymentVoucher = new PaymentVoucher()
                {
                    VoucherDate = dto.VoucherDate,
                    EmployeeId = dto.EmployeeId,
                    //NetAmount = (long)dto.NetAmount,
                    // VatAmount = (long)dto.VATAmount,
                    TotalAmount = (long)dto.PaidAmountOverpayment,
                    CashSessionId = dto.CashSessionId,
                    PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                    //  VoucherNo = dto. need see any role used on this point generate it from data base or create algorithm to it
                    VoucherNo = await GenerateVoucherNumber()
                };

                // i will build foreach replace this code 
                paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = dto.PayableAccountIdOverpayment.Value,
                    Amount = (long)dto.PaidAmountOverpayment,
                });
                paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = dto.CashAccountId, // bank
                    Amount = (long)dto.PaidAmountOverpayment
                });


                var journalEntry = new JournalEntryPostModel()
                {
                    //Reference = what should write her 
                    Reference = $"PV-{paymentVoucher.VoucherNo}", // and her need to more clarification for  reference 
                    EntryDate = dto.VoucherDate,
                };
                // will also replace this code with foreach 

                journalEntry.Lines = new List<JournalEntryLinePostModel>
                {
                    new JournalEntryLinePostModel()
                    {
                        FinancialAccountId = dto.PayableAccountIdOverpayment.Value,
                        Debit = (long)dto.PaidAmountOverpayment-(long)OverAmount,
                        Credit =0
                    },
                    new JournalEntryLinePostModel()
                    {
                        FinancialAccountId = dto.CashAccountId, //bank
                        Debit = 0,
                        Credit =(long)dto.PaidAmountOverpayment
                    },
                    new JournalEntryLinePostModel()
                    {
                        FinancialAccountId = dto.SupplierAdvancesIdOverpayment.Value,
                        Debit = (long)OverAmount,
                        Credit =0
                    },
                };
                var result = await journalEntryService.PostJournalEntry(journalEntry);
                if (result != JournalEntryError.CreatedSuccessfully)
                    return false;

                await context.PaymentVouchers.AddAsync(paymentVoucher);
                await context.SaveChangesAsync();

                return true;

            }

            return default;
        }
        public async Task<bool> AdvancePayment(CreatePaymentVoucherDto dto)
        {
            if (dto.SettlementType is not SettlementType.AdvancePayment)
                return false;

            // 1️⃣ Get Supplier
            var supplier = await supplierRepository.GetSupplierById(dto.PayableAccountId.Value);
            if (supplier is null)
                return false;

            if (dto.PaidAmount < 0)
                return false;



            ////
            var PrepaidExpensesAccount = await context.FinancialAccounts
                        .FirstAsync(a => a.SystemRole == SystemAccountType.PrepaidExpenses);

            ///
            var paymentVoucher = new PaymentVoucher
            {
                VoucherDate = dto.VoucherDate,
                EmployeeId = dto.EmployeeId,
                PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                CashSessionId = dto.CashSessionId,
                TotalAmount = (long)dto.PaidAmount,
                VoucherNo = await GenerateVoucherNumber()
            };

            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.BankAccountId ?? dto.CashAccountId,
                Amount = (long)dto.PaidAmount
            });
            paymentVoucher.PaymentVoucherLines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = dto.PayableAccountId!.Value,
                Amount = (long)dto.PaidAmount,
            });

            var journalEntry = new JournalEntryPostModel
            {
                EntryDate = dto.VoucherDate,
                Reference = $"Withholding Payment - Supplier #{supplier.Id}"
            };

            journalEntry.Lines = new List<JournalEntryLinePostModel>
            {
                new()
                {
                    FinancialAccountId = dto.BankAccountId ?? dto.CashAccountId,
                    Credit = (long)dto.PaidAmount,
                    Debit  =0,
                },
                new()
                {
                    FinancialAccountId = PrepaidExpensesAccount.Id,
                    Credit = 0,
                    Debit = (long)dto.PaidAmount
                }
            };
            var result = await journalEntryService.PostJournalEntry(journalEntry);
            if (result != JournalEntryError.CreatedSuccessfully)
                return false;

            await context.PaymentVouchers.AddAsync(paymentVoucher);
            await context.SaveChangesAsync();

            return true;

            return default;
        }

        public async Task<bool> ReversePaymentVoucher(int voucherId, long? PartialPayment = null)
        {
            var payment = await context.PaymentVouchers.Include(e => e.JournalEntry)
                                        .SingleOrDefaultAsync(e => e.Id == voucherId);

            if (payment is null)
                return false;

            if (payment.JournalEntry.IsReversal)
                return false;


            var paymentIsReconciled = new Random().Next(1, 100);


            if (paymentIsReconciled > 700)
                return false;


            payment.IsReversed = true;

            ReversalOptions reversalOptions = null;

            if (PartialPayment is not null)
            {
                reversalOptions = new ReversalOptions()
                {
                    PartialPayment = PartialPayment,
                    ReversalDate = DateTime.Now,
                };             
            }

            var result = await journalEntryService.ReverseJournalEntry(payment.JournalEntryId, reversalOptions);

            if (!result)
                return false;

            await context.SaveChangesAsync();

            return true;
        }
    }
}
