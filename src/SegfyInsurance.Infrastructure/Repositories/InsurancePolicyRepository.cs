using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SegfyInsurance.Domain.Entities;
using SegfyInsurance.Domain.Interfaces;
using SegfyInsurance.Infrastructure.Data;

namespace SegfyInsurance.Infrastructure.Repositories
{
    public class InsurancePolicyRepository : IInsurancePolicyRepository
    {
        private readonly AppDbContext _context;

        public InsurancePolicyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InsurancePolicy?> GetByIdAsync(Guid id)
        {
            return await _context.Policies.FindAsync(id);
        }

        public async Task<IEnumerable<InsurancePolicy>> GetAllAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<IEnumerable<InsurancePolicy>> GetExpiringIn30DaysAsync()
        {
            // Raw SQL query to fetch active policies expiring within the next 30 days.
            // Under SQLite, date('now') returns the current date in YYYY-MM-DD.
            // Status = 0 corresponds to PolicyStatus.Ativa.
            var sql = @"
                SELECT * FROM Policies 
                WHERE Status = 0 
                  AND Date(DataFimVigencia) >= Date('now') 
                  AND Date(DataFimVigencia) <= Date('now', '+30 days')";

            return await _context.Policies
                .FromSqlRaw(sql)
                .ToListAsync();
        }

        public async Task AddAsync(InsurancePolicy policy)
        {
            await _context.Policies.AddAsync(policy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InsurancePolicy policy)
        {
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var policy = await GetByIdAsync(id);
            if (policy != null)
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNextSequenceForYearAsync(int year)
        {
            var prefix = $"SEG-{year}-";
            var policyNumbers = await _context.Policies
                .Where(p => p.NumeroApolice.StartsWith(prefix))
                .Select(p => p.NumeroApolice)
                .ToListAsync();

            if (!policyNumbers.Any())
            {
                return 1;
            }

            var maxSequence = policyNumbers
                .Select(num => {
                    var parts = num.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
                    {
                        return seq;
                    }
                    return 0;
                })
                .Max();

            return maxSequence + 1;
        }
    }
}
