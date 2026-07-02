using System;

namespace SegfyInsurance.Application.Dtos
{
    public class UpdateInsurancePolicyDto
    {
        public string CpfCnpjSegurado { get; set; } = string.Empty;
        public string PlacaVeiculo { get; set; } = string.Empty;
        public decimal ValorPremio { get; set; }
        public DateTime DataInicioVigencia { get; set; }
        public DateTime DataFimVigencia { get; set; }
    }
}
