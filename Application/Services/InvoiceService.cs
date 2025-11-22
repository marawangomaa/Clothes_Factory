using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class InvoiceService
    {
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<InvoiceModel> _invoiceModelRepo;
        private readonly IRepository<Model> _modelRepo;
        private readonly IRepository<Storage> _storageRepo;
        private readonly IRepository<Clinte> _clientRepo;
        private readonly IRepository<Bank> _bankRepo;
        private readonly IRepository<BankTransaction> _transactionRepo;
        private readonly BankService _bankService;

        public InvoiceService(
            IRepository<Invoice> invoiceRepo,
            IRepository<InvoiceModel> invoiceModelRepo,
            IRepository<Model> modelRepo,
            IRepository<Storage> storageRepo,
            IRepository<Clinte> clientRepo,
            IRepository<Bank> bankRepo,
            IRepository<BankTransaction> transactionRepo,
            BankService bankService)
        {
            _invoiceRepo = invoiceRepo;
            _invoiceModelRepo = invoiceModelRepo;
            _modelRepo = modelRepo;
            _storageRepo = storageRepo;
            _clientRepo = clientRepo;
            _bankRepo = bankRepo;
            _transactionRepo = transactionRepo;
            _bankService = bankService;
        }

        // ================================================================
        // 1) LIST OF CLIENT INVOICES
        // ================================================================
        public async Task<List<InvoiceDisplayDto>> GetClientInvoicesAsync(int clientId)
        {
            var invoices = (await _invoiceRepo.GetAllAsync())
                .Where(i => i.ClinteID == clientId)
                .OrderByDescending(i => i.Date)
                .ToList();

            var invoiceModels = (await _invoiceModelRepo.GetAllAsync()).ToList();
            var models = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.ID);
            var transactions = (await _transactionRepo.GetAllAsync()).ToList();

            var result = new List<InvoiceDisplayDto>();

            foreach (var inv in invoices)
            {
                var items = invoiceModels
                    .Where(im => im.InvoiceID == inv.ID)
                    .Select(im =>
                    {
                        var model = models.ContainsKey(im.ModelID) ? models[im.ModelID] : null;
                        decimal price = im.UnitPrice > 0 ? im.UnitPrice : model?.SellPrice ?? 0;

                        return new InvoiceItemViewDto
                        {
                            ModelId = im.ModelID,
                            ModelName = model != null
                                ? (string.IsNullOrWhiteSpace(model.Name) ? model.Code : model.Name)
                                : $"Model-{im.ModelID}",
                            Quantity = im.Quantity,
                            UnitPrice = price
                        };
                    })
                    .ToList();

                decimal total = items.Sum(i => i.Total);

                var relatedTx = transactions.Where(t => t.InvoiceID == inv.ID);

                decimal income = relatedTx.Where(t => t.Type == "دخل").Sum(t => t.Amount);
                decimal outcome = relatedTx.Where(t => t.Type == "خارج").Sum(t => t.Amount);

                decimal paid = Math.Max(income - outcome, 0);
                decimal remaining = inv.InvoiceType == "مرتجع" ? 0 : Math.Max(total - paid, 0);

                result.Add(new InvoiceDisplayDto
                {
                    Id = inv.ID,
                    Type = inv.InvoiceType ?? "غير معرف",
                    Date = inv.Date,
                    Items = items,
                    TotalAmount = total,
                    PaidAmount = paid,
                    PaymentMethod = inv.PaymentMethod // string now
                });
            }

            return result;
        }

        // ================================================================
        // 2) DETAILED CLIENT INVOICES
        // ================================================================
        public async Task<List<InvoiceViewDto>> GetClientInvoicesDetailedAsync(int clientId)
        {
            var invoices = (await _invoiceRepo.GetAllAsync())
                .Where(i => i.ClinteID == clientId)
                .OrderByDescending(i => i.Date)
                .ToList();

            var invoiceModels = (await _invoiceModelRepo.GetAllAsync()).ToList();
            var allModels = (await _modelRepo.GetAllAsync()).ToDictionary(m => m.ID);
            var transactions = (await _transactionRepo.GetAllAsync()).ToList();

            var result = new List<InvoiceViewDto>();

            foreach (var inv in invoices)
            {
                var items = invoiceModels
                    .Where(m => m.InvoiceID == inv.ID)
                    .Select(m =>
                    {
                        var model = allModels.ContainsKey(m.ModelID) ? allModels[m.ModelID] : null;
                        decimal price = m.UnitPrice > 0 ? m.UnitPrice : model?.SellPrice ?? 0;

                        return new InvoiceItemViewDto
                        {
                            ModelId = m.ModelID,
                            ModelName = model != null
                                ? (string.IsNullOrWhiteSpace(model.Name) ? model.Code : model.Name)
                                : $"Model-{m.ModelID}",
                            Quantity = m.Quantity,
                            UnitPrice = price
                        };
                    })
                    .ToList();

                var relatedTx = transactions.Where(t => t.InvoiceID == inv.ID);
                decimal income = relatedTx.Where(t => t.Type == "دخل").Sum(t => t.Amount);
                decimal outcome = relatedTx.Where(t => t.Type == "خارج").Sum(t => t.Amount);

                decimal paidAmount = Math.Max(income - outcome, 0);

                result.Add(new InvoiceViewDto
                {
                    InvoiceId = inv.ID,
                    InvoiceType = inv.InvoiceType ?? "غير معرف",
                    Date = inv.Date,
                    Items = items,
                    PaidAmount = paidAmount,
                    PaymentMethod = inv.PaymentMethod // string now
                });
            }

            return result;
        }

        // ================================================================
        // 3) GET ALL CLIENT SOLD ITEMS
        // ================================================================
        public async Task<List<SellItemDto>> GetClientSellItemsAsync(int clientId)
        {
            var soldInvoices = (await _invoiceRepo.GetAllAsync())
                .Where(i => i.ClinteID == clientId && i.InvoiceType == "فاتورة جديدة")
                .ToList();

            var soldInvoiceIds = soldInvoices.Select(i => i.ID).ToList();

            var soldItems = (await _invoiceModelRepo.GetAllAsync())
                .Where(im => soldInvoiceIds.Contains(im.InvoiceID))
                .ToList();

            // Previous returns
            var returnInvoices = (await _invoiceRepo.GetAllAsync())
                .Where(i => i.ClinteID == clientId && i.InvoiceType == "مرتجع")
                .Select(i => i.ID)
                .ToList();

            var returnedItems = (await _invoiceModelRepo.GetAllAsync())
                .Where(im => returnInvoices.Contains(im.InvoiceID))
                .ToList();

            var result = new List<SellItemDto>();

            // Group by ModelId
            var groupedSold = soldItems.GroupBy(s => s.ModelID);

            foreach (var group in groupedSold)
            {
                var modelId = group.Key;
                var model = await _modelRepo.GetByIdAsync(modelId);
                if (model == null) continue;

                int totalSold = group.Sum(g => g.Quantity);
                int totalReturned = returnedItems.Where(r => r.ModelID == modelId).Sum(r => r.Quantity);

                int currentQty = totalSold - totalReturned;

                // include items even if currentQty = 0 if you want
                result.Add(new SellItemDto
                {
                    ItemId = modelId,
                    ItemName = string.IsNullOrWhiteSpace(model.Name) ? model.Code : model.Name,
                    Quantity = Math.Max(currentQty, 0),
                    Price = group.First().UnitPrice
                });
            }

            return result;
        }



        // ================================================================
        // 4) ADD SELL INVOICE — FIXED (PURE STRINGS)
        // ================================================================
        public async Task AddSellInvoiceAsync(
            int clientId,
            List<InvoiceItemViewDto> items,
            decimal totalAmount,
            string paymentType,
            decimal paymentAmount = 0)
        {
            var client = await _clientRepo.GetByIdAsync(clientId) ?? throw new Exception("العميل غير موجود");
            var bank = await _bankService.GetOrCreateBankAsync();

            // ----------- CHECK STOCK FIRST -------------
            foreach (var item in items)
            {
                var model = await _modelRepo.GetByIdAsync(item.ModelId)
                    ?? throw new Exception("الموديل غير موجود");

                var store = model.StorageID.HasValue
                    ? await _storageRepo.GetByIdAsync(model.StorageID.Value)
                    : (await _storageRepo.GetAllAsync()).FirstOrDefault(s => s.Product_Name == model.Name);

                if (store == null)
                    throw new Exception($"الموديل غير موجود: {model.Name}");

                if (store.Number_Of_Products < item.Quantity)
                    throw new Exception($"العدد غير كافى من {model.Name}");
            }

            // ----------- CREATE INVOICE AFTER STOCK CHECK -------------
            var invoice = new Invoice
            {
                Date = DateTime.Now,
                Number = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                InvoiceType = "فاتورة جديدة",
                TotalAmount = totalAmount,
                ClinteID = clientId,
                BankID = bank.ID,
                PaymentMethod = paymentType,
                Type = "فاتورة جديدة"
            };

            await _invoiceRepo.AddAsync(invoice);
            await _invoiceRepo.SaveChangesAsync();

            // ----------- SUBTRACT STOCK & ADD ITEMS -------------
            foreach (var item in items)
            {
                var model = await _modelRepo.GetByIdAsync(item.ModelId);
                var store = model.StorageID.HasValue
                    ? await _storageRepo.GetByIdAsync(model.StorageID.Value)
                    : (await _storageRepo.GetAllAsync()).First(s => s.Product_Name == model.Name);

                store.Number_Of_Products -= item.Quantity;
                _storageRepo.Update(store);

                await _invoiceModelRepo.AddAsync(new InvoiceModel
                {
                    InvoiceID = invoice.ID,
                    ModelID = item.ModelId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            await _storageRepo.SaveChangesAsync();
            await _invoiceModelRepo.SaveChangesAsync();

            // ----------- HANDLE CLIENT DEBT & PAYMENT -------------
            client.Debt += totalAmount;

            if (paymentAmount > 0)
            {
                decimal pay = Math.Min(paymentAmount, totalAmount);

                bank.TotalAmount += pay;

                await _transactionRepo.AddAsync(new BankTransaction
                {
                    Date = DateTime.Now,
                    Type = "دخل",
                    Amount = pay,
                    TotalAfterTransaction = bank.TotalAmount,
                    InvoiceID = invoice.ID,
                    BankID = bank.ID
                });

                _bankRepo.Update(bank);
                await _transactionRepo.SaveChangesAsync();

                client.Debt -= pay;
                if (client.Debt < 0) client.Debt = 0;
            }

            _clientRepo.Update(client);
            await _clientRepo.SaveChangesAsync();
        }


        // ================================================================
        // 5) ADD RETURN INVOICE (string payment)
        // ================================================================
        public async Task AddReturnInvoiceAsync(int clientId, List<ReturnItemDto> items, decimal totalRefund)
        {
            if (!items.Any())
                throw new Exception("لا يوجد تطابق فى عدد القطع");

            var client = await _clientRepo.GetByIdAsync(clientId)
                         ?? throw new Exception("العميل غير موجود");

            var bank = await _bankService.GetOrCreateBankAsync();

            var invoice = new Invoice
            {
                Date = DateTime.Now,
                Number = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                InvoiceType = "مرتجع",
                TotalAmount = totalRefund,
                ClinteID = clientId,
                BankID = bank.ID,
                PaymentMethod = "مرتجع",
                Type = "مرتجع"
            };

            await _invoiceRepo.AddAsync(invoice);
            await _invoiceRepo.SaveChangesAsync();

            foreach (var item in items)
            {
                var model = await _modelRepo.GetByIdAsync(item.ItemId)
                            ?? throw new Exception($"الموديل غير موجود: {item.ItemName}");

                var storage = model.StorageID.HasValue
                    ? await _storageRepo.GetByIdAsync(model.StorageID.Value)
                    : (await _storageRepo.GetAllAsync()).FirstOrDefault(s => s.Product_Name == model.Name);

                if (storage == null)
                    throw new Exception($"الموديل غير موجود فى المخزن: {model.Name}");

                storage.Number_Of_Products += item.ReturnQty;
                _storageRepo.Update(storage);

                await _invoiceModelRepo.AddAsync(new InvoiceModel
                {
                    InvoiceID = invoice.ID,
                    ModelID = model.ID,
                    Quantity = item.ReturnQty,
                    UnitPrice = item.Price
                });
            }

            await _storageRepo.SaveChangesAsync();
            await _invoiceModelRepo.SaveChangesAsync();

            // ====== BANK OUTCOME ======
            bank.TotalAmount -= totalRefund;
            _bankRepo.Update(bank);

            await _transactionRepo.AddAsync(new BankTransaction
            {
                Date = DateTime.Now,
                Type = "خارج",                 // <--- mark as Outcome
                Amount = totalRefund,
                TotalAfterTransaction = bank.TotalAmount,
                InvoiceID = invoice.ID,
                BankID = bank.ID
            });

            // ====== CLIENT DEBT ======
            client.Debt -= totalRefund;
            if (client.Debt < 0) client.Debt = 0;
            _clientRepo.Update(client);

            await _transactionRepo.SaveChangesAsync();
            await _bankRepo.SaveChangesAsync();
            await _clientRepo.SaveChangesAsync();
        }



        // ================================================================
        // 6) PAYMENT ONLY
        // ================================================================
        public async Task AddPaymentAsync(int clientId, decimal amount, string paymentMethod = "دفعة")
        {
            if (amount <= 0)
                throw new Exception("رقم غير صالح");

            // load client and bank
            var client = await _clientRepo.GetByIdAsync(clientId) ?? throw new Exception("العميل غير موجود");
            var bank = await _bankService.GetOrCreateBankAsync();

            // 1) create payment invoice so we have an Invoice.ID to link to the transaction
            var invoice = new Invoice
            {
                Date = DateTime.Now,
                Number = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                PaymentMethod = paymentMethod ?? "دفعة",   // store the chosen method
                InvoiceType = "دفعة",
                Type = "دفعة",                              // if you use Type string for something else adjust accordingly
                TotalAmount = amount,
                ClinteID = clientId,
                BankID = bank.ID
            };

            await _invoiceRepo.AddAsync(invoice);
            await _invoiceRepo.SaveChangesAsync(); // now invoice.ID is available

            // 2) update bank total and create bank transaction linked to invoice
            bank.TotalAmount += amount;
            _bankRepo.Update(bank);

            var tx = new BankTransaction
            {
                Date = DateTime.Now,
                Type = "دخل",
                Amount = amount,
                TotalAfterTransaction = bank.TotalAmount,
                InvoiceID = invoice.ID,
                BankID = bank.ID
            };

            await _transactionRepo.AddAsync(tx);
            await _transactionRepo.SaveChangesAsync();

            // 3) decrease client debt
            client.Debt -= amount;
            if (client.Debt < 0) client.Debt = 0;

            _clientRepo.Update(client);
            await _clientRepo.SaveChangesAsync();
        }


        // ================================================================
        // 7) RETURN INVOICE VM API
        // ================================================================
        public async Task<ServiceResult<bool>> CreateReturnInvoiceAsync(int clientId, List<ReturnItemDto> items)
        {
            try
            {
                if (!items.Any())
                    return ServiceResult<bool>.Fail("لا يوجد موديل للارجاع");

                decimal refund = items.Sum(i => i.Total);

                await AddReturnInvoiceAsync(clientId, items, refund); // <-- pass ReturnItemDto directly

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(ex.Message);
            }
        }

    }
}
