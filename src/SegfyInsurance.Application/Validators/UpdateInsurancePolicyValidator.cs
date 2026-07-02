using System;
using System.Text.RegularExpressions;
using FluentValidation;
using SegfyInsurance.Application.Dtos;

namespace SegfyInsurance.Application.Validators
{
    public class UpdateInsurancePolicyValidator : AbstractValidator<UpdateInsurancePolicyDto>
    {
        public UpdateInsurancePolicyValidator()
        {
            RuleFor(x => x.CpfCnpjSegurado)
                .NotEmpty().WithMessage("CPF/CNPJ do segurado é obrigatório.")
                .Must(DocumentValidator.IsValidCpfCnpj).WithMessage("CPF/CNPJ do segurado é inválido.");

            RuleFor(x => x.PlacaVeiculo)
                .NotEmpty().WithMessage("Placa do veículo é obrigatória.")
                .Must(BeAValidPlate).WithMessage("Placa do veículo é inválida. Use o padrão AAA-9999 ou AAA9A99.");

            RuleFor(x => x.ValorPremio)
                .GreaterThan(0).WithMessage("O valor do prêmio deve ser maior que zero.");

            RuleFor(x => x.DataInicioVigencia)
                .NotEmpty().WithMessage("Data de início de vigência é obrigatória.");

            RuleFor(x => x.DataFimVigencia)
                .NotEmpty().WithMessage("Data de término de vigência é obrigatória.")
                .GreaterThan(x => x.DataInicioVigencia).WithMessage("A data de término deve ser posterior à data de início.");
        }

        private bool BeAValidPlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return false;
            
            var cleanPlate = plate.Replace("-", "").Replace(" ", "").Trim().ToUpper();

            var traditionalRegex = new Regex(@"^[A-Z]{3}\d{4}$");
            var mercosulRegex = new Regex(@"^[A-Z]{3}\d[A-Z]\d{2}$");

            return traditionalRegex.IsMatch(cleanPlate) || mercosulRegex.IsMatch(cleanPlate);
        }
    }
}
