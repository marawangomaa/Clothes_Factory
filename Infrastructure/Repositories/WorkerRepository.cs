using Domain.Entities;
using Domain.Repository_Interfaces;
using Infrastructure.Data.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class WorkerRepository : Repository<Worker>, IWorkerRepository
    {
        public WorkerRepository(ClothesSystemDbContext context) : base(context) { }

        public async Task<IEnumerable<Worker>> GetAllWithDetailsAsync()
        {
            return await _context.Set<Worker>()
                                 .Include(w => w.DailyPieces)
                                 .Include(w => w.BankTransactions)
                                 .ToListAsync();
        }
    }

}
