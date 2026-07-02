using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SegfyInsurance.Application.Dtos;
using SegfyInsurance.Application.Services;
using SegfyInsurance.Application.Validators;
using SegfyInsurance.Domain.Entities;
using SegfyInsurance.Domain.Enums;
using SegfyInsurance.Domain.Interfaces;
using Xunit;

namespace SegfyInsurance.Tests.Services
{
    public class InsurancePolicyServiceTests
    {
        private readonly Mock<IInsurancePolicyRepository> _repoMock;
        private readonly InsurancePolicyService _service;

        public InsurancePolicyServiceTests()
        {
            _repoMock = new Mock<IInsurancePolicyRepository>();
            _service = new InsurancePolicyService(_repoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldGeneratePolicyNumberAndSave()
        {
            // Arrange
            var dto = new CreateInsurancePolicyDto
            {
                CpfCnpjSegurado = "11.222.333/0001-81", // Valid CNPJ
                PlacaVeiculo = "ABC-1234",
                ValorPremio = 150.50m,
                DataInicioVigencia = DateTime.Today,
                DataFimVigencia = DateTime.Today.AddYears(1)
            };

            var currentYear = DateTime.Today.Year;
            _repoMock.Setup(r => r.GetNextSequenceForYearAsync(currentYear))
                .ReturnsAsync(5); // Mock next sequence to be 5

            _repoMock.Setup(r => r.AddAsync(It.IsAny<InsurancePolicy>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"SEG-{currentYear}-0005", result.NumeroApolice);
            Assert.Equal("11222333000181", result.CpfCnpjSegurado.Replace(".", "").Replace("/", "").Replace("-", ""));
            Assert.Equal("ABC-1234", result.PlacaVeiculo);
            Assert.Equal(150.50m, result.ValorPremio);
            Assert.Equal("Ativa", result.Status);

            _repoMock.Verify(r => r.AddAsync(It.Is<InsurancePolicy>(p => 
                p.NumeroApolice == $"SEG-{currentYear}-0005" &&
                p.CpfCnpjSegurado == "11222333000181" &&
                p.PlacaVeiculo == "ABC-1234" &&
                p.ValorPremio == 150.50m &&
                p.Status == PolicyStatus.Ativa
            )), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldAutoExpire_WhenDateEndedAndStatusIsActive()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var expiredPolicy = new InsurancePolicy
            {
                Id = policyId,
                NumeroApolice = "SEG-2025-0001",
                CpfCnpjSegurado = "12345678909",
                PlacaVeiculo = "ABC-1234",
                ValorPremio = 100m,
                DataInicioVigencia = DateTime.Today.AddMonths(-12),
                DataFimVigencia = DateTime.Today.AddDays(-1), // Ended yesterday
                Status = PolicyStatus.Ativa
            };

            _repoMock.Setup(r => r.GetByIdAsync(policyId))
                .ReturnsAsync(expiredPolicy);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<InsurancePolicy>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetByIdAsync(policyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Expirada", result.Status);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<InsurancePolicy>(p => 
                p.Id == policyId && p.Status == PolicyStatus.Expirada
            )), Times.Once);
        }

        [Theory]
        // Valid vs Invalid documents (CPF/CNPJ check digits algorithms)
        [InlineData("12345678909", true)]      // Valid CPF
        [InlineData("12345678900", false)]     // Invalid CPF
        [InlineData("11222333000181", true)]   // Valid CNPJ
        [InlineData("11222333000100", false)]  // Invalid CNPJ
        public void DocumentValidator_ShouldValidateCorrectly(string doc, bool expectedResult)
        {
            // Act
            var result = DocumentValidator.IsValidCpfCnpj(doc);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("ABC-1234", true)]  // Traditional with dash
        [InlineData("abc-1234", true)]  // Traditional lowercase with dash
        [InlineData("ABC1234", true)]   // Traditional no dash
        [InlineData("ABC1D23", true)]   // Mercosul
        [InlineData("abc1d23", true)]   // Mercosul lowercase
        [InlineData("ABC-123", false)]  // Too short traditional
        [InlineData("ABCD1234", false)] // Too long
        [InlineData("123-ABCD", false)] // Reversed
        public void CreateInsurancePolicyValidator_ShouldValidatePlates(string plate, bool expectedValid)
        {
            // Arrange
            var validator = new CreateInsurancePolicyValidator();
            var dto = new CreateInsurancePolicyDto
            {
                CpfCnpjSegurado = "11.222.333/0001-81", // Valid CNPJ
                PlacaVeiculo = plate,
                ValorPremio = 100m,
                DataInicioVigencia = DateTime.Today,
                DataFimVigencia = DateTime.Today.AddYears(1)
            };

            // Act
            var result = validator.Validate(dto);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Fact]
        public void CreateInsurancePolicyValidator_ShouldFail_WhenEndDateIsBeforeOrEqualsStartDate()
        {
            // Arrange
            var validator = new CreateInsurancePolicyValidator();
            var dto = new CreateInsurancePolicyDto
            {
                CpfCnpjSegurado = "11.222.333/0001-81", // Valid CNPJ
                PlacaVeiculo = "ABC-1234",
                ValorPremio = 100m,
                DataInicioVigencia = DateTime.Today,
                DataFimVigencia = DateTime.Today // Equals, should fail
            };

            // Act
            var result = validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.DataFimVigencia));
        }
    }
}
