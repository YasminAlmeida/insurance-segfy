using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SegfyInsurance.Application.Dtos;

namespace SegfyInsurance.Application.Services
{
    public interface IInsurancePolicyService
    {
        Task<InsurancePolicyDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<InsurancePolicyDto>> GetAllAsync();
        Task<IEnumerable<InsurancePolicyDto>> GetExpiringIn30DaysAsync();
        Task<InsurancePolicyDto> CreateAsync(CreateInsurancePolicyDto dto);
        Task<InsurancePolicyDto?> UpdateAsync(Guid id, UpdateInsurancePolicyDto dto);
        Task<bool> CancelAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
