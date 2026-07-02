using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SegfyInsurance.Application.Dtos;
using SegfyInsurance.Domain.Entities;
using SegfyInsurance.Domain.Enums;
using SegfyInsurance.Domain.Interfaces;

namespace SegfyInsurance.Application.Services
{
    public class InsurancePolicyService : IInsurancePolicyService
    {
        private readonly IInsurancePolicyRepository _repository;

        public InsurancePolicyService(IInsurancePolicyRepository repository)
        {
            _repository = repository;
        }

        public async Task<InsurancePolicyDto?> GetByIdAsync(Guid id)
        {
            var policy = await _repository.GetByIdAsync(id);
            if (policy == null) return null;

            await CheckAndAutoExpireAsync(policy);

            return MapToDto(policy);
        }

        public async Task<IEnumerable<InsurancePolicyDto>> GetAllAsync()
        {
            var policies = await _repository.GetAllAsync();
            var dtoList = new List<InsurancePolicyDto>();

            foreach (var policy in policies)
            {
                await CheckAndAutoExpireAsync(policy);
                dtoList.Add(MapToDto(policy));
            }

            return dtoList;
        }

        public async Task<IEnumerable<InsurancePolicyDto>> GetExpiringIn30DaysAsync()
        {
            var policies = await _repository.GetExpiringIn30DaysAsync();
            var dtoList = new List<InsurancePolicyDto>();

            foreach (var policy in policies)
            {
                await CheckAndAutoExpireAsync(policy);
                dtoList.Add(MapToDto(policy));
            }

            return dtoList;
        }

        public async Task<InsurancePolicyDto> CreateAsync(CreateInsurancePolicyDto dto)
        {
            var year = DateTime.Today.Year;
            var sequence = await _repository.GetNextSequenceForYearAsync(year);
            var policyNumber = $"SEG-{year}-{sequence:D4}";

            var policy = new InsurancePolicy
            {
                Id = Guid.NewGuid(),
                NumeroApolice = policyNumber,
                CpfCnpjSegurado = CleanDocument(dto.CpfCnpjSegurado),
                PlacaVeiculo = CleanPlate(dto.PlacaVeiculo),
                ValorPremio = dto.ValorPremio,
                DataInicioVigencia = dto.DataInicioVigencia.Date,
                DataFimVigencia = dto.DataFimVigencia.Date,
                Status = PolicyStatus.Ativa
            };

            policy.CheckAndAutoExpire();

            await _repository.AddAsync(policy);

            return MapToDto(policy);
        }

        public async Task<InsurancePolicyDto?> UpdateAsync(Guid id, UpdateInsurancePolicyDto dto)
        {
            var policy = await _repository.GetByIdAsync(id);
            if (policy == null) return null;

            policy.CpfCnpjSegurado = CleanDocument(dto.CpfCnpjSegurado);
            policy.PlacaVeiculo = CleanPlate(dto.PlacaVeiculo);
            policy.ValorPremio = dto.ValorPremio;
            policy.DataInicioVigencia = dto.DataInicioVigencia.Date;
            policy.DataFimVigencia = dto.DataFimVigencia.Date;

            policy.CheckAndAutoExpire();

            await _repository.UpdateAsync(policy);

            return MapToDto(policy);
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var policy = await _repository.GetByIdAsync(id);
            if (policy == null) return false;

            policy.Status = PolicyStatus.Cancelada;
            await _repository.UpdateAsync(policy);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var policy = await _repository.GetByIdAsync(id);
            if (policy == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        private async Task CheckAndAutoExpireAsync(InsurancePolicy policy)
        {
            if (policy.Status == PolicyStatus.Ativa && policy.DataFimVigencia.Date < DateTime.Today)
            {
                policy.Status = PolicyStatus.Expirada;
                await _repository.UpdateAsync(policy);
            }
        }

        private static string CleanDocument(string doc)
        {
            if (string.IsNullOrWhiteSpace(doc)) return string.Empty;
            return new string(doc.Where(char.IsDigit).ToArray());
        }

        private static string CleanPlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return string.Empty;
            var clean = plate.Replace("-", "").Replace(" ", "").Trim().ToUpper();
            if (clean.Length == 7)
            {
                if (char.IsDigit(clean[3]) && char.IsDigit(clean[4]) && char.IsDigit(clean[5]) && char.IsDigit(clean[6]))
                {
                    return $"{clean.Substring(0, 3)}-{clean.Substring(3)}";
                }
            }
            return clean;
        }

        private static InsurancePolicyDto MapToDto(InsurancePolicy policy)
        {
            return new InsurancePolicyDto
            {
                Id = policy.Id,
                NumeroApolice = policy.NumeroApolice,
                CpfCnpjSegurado = FormatDocument(policy.CpfCnpjSegurado),
                PlacaVeiculo = policy.PlacaVeiculo,
                ValorPremio = policy.ValorPremio,
                DataInicioVigencia = policy.DataInicioVigencia,
                DataFimVigencia = policy.DataFimVigencia,
                Status = policy.Status.ToString()
            };
        }

        private static string FormatDocument(string doc)
        {
            if (string.IsNullOrWhiteSpace(doc)) return string.Empty;
            
            if (doc.Length == 11) // CPF
            {
                return $"{doc.Substring(0, 3)}.{doc.Substring(3, 3)}.{doc.Substring(6, 3)}-{doc.Substring(9, 2)}";
            }
            if (doc.Length == 14) // CNPJ
            {
                return $"{doc.Substring(0, 2)}.{doc.Substring(2, 3)}.{doc.Substring(5, 3)}/{doc.Substring(8, 4)}-{doc.Substring(12, 2)}";
            }
            return doc;
        }
    }
}
