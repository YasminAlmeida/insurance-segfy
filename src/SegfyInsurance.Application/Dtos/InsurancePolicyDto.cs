using System;

namespace SegfyInsurance.Application.Dtos
{
    public class InsurancePolicyDto
    {
        public Guid Id { get; set; }
        public string NumeroApolice { get; set; } = string.Empty;
        public string CpfCnpjSegurado { get; set; } = string.Empty;
        public string PlacaVeiculo { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateTime DataInicioVigencia { get; set; }
        public DateTime DataFimVigencia { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
