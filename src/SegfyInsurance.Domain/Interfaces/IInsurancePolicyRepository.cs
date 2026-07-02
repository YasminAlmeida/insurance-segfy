using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SegfyInsurance.Domain.Entities;

namespace SegfyInsurance.Domain.Interfaces
{
    public interface IInsurancePolicyRepository
    {
        Task<InsurancePolicy?> GetByIdAsync(Guid id);
        Task<IEnumerable<InsurancePolicy>> GetAllAsync();
        Task<IEnumerable<InsurancePolicy>> GetExpiringIn30DaysAsync();
        Task AddAsync(InsurancePolicy policy);
        Task UpdateAsync(InsurancePolicy policy);
        Task DeleteAsync(Guid id);
        Task<int> GetNextSequenceForYearAsync(int year);
    }
}
