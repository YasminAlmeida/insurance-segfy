using System;
using SegfyInsurance.Domain.Enums;

namespace SegfyInsurance.Domain.Entities
{
    public class InsurancePolicy
    {
        public Guid Id { get; set; }
        public string NumeroApolice { get; set; } = string.Empty;
        public string CpfCnpjSegurado { get; set; } = string.Empty;
        public string PlacaVeiculo { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateTime DataInicioVigencia { get; set; }
        public DateTime DataFimVigencia { get; set; }
        public PolicyStatus Status { get; set; }

        public void CheckAndAutoExpire()
        {
            if (Status == PolicyStatus.Ativa && DataFimVigencia.Date < DateTime.Today)
            {
                Status = PolicyStatus.Expirada;
            }
        }
    }
}
