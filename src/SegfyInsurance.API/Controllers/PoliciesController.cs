using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SegfyInsurance.Application.Dtos;
using SegfyInsurance.Application.Services;

namespace SegfyInsurance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IInsurancePolicyService _policyService;
        private readonly IValidator<CreateInsurancePolicyDto> _createValidator;
        private readonly IValidator<UpdateInsurancePolicyDto> _updateValidator;

        public PoliciesController(
            IInsurancePolicyService policyService,
            IValidator<CreateInsurancePolicyDto> createValidator,
            IValidator<UpdateInsurancePolicyDto> updateValidator)
        {
            _policyService = policyService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsurancePolicyDto>>> GetAll()
        {
            var policies = await _policyService.GetAllAsync();
            return Ok(policies);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<InsurancePolicyDto>> GetById(Guid id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null) return NotFound(new { message = "Apólice não encontrada." });
            return Ok(policy);
        }

        [HttpGet("expiring-soon")]
        public async Task<ActionResult<IEnumerable<InsurancePolicyDto>>> GetExpiringSoon()
        {
            var policies = await _policyService.GetExpiringIn30DaysAsync();
            return Ok(policies);
        }

        [HttpPost]
        public async Task<ActionResult<InsurancePolicyDto>> Create([FromBody] CreateInsurancePolicyDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var createdPolicy = await _policyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPolicy.Id }, createdPolicy);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<InsurancePolicyDto>> Update(Guid id, [FromBody] UpdateInsurancePolicyDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var updatedPolicy = await _policyService.UpdateAsync(id, dto);
            if (updatedPolicy == null) return NotFound(new { message = "Apólice não encontrada." });

            return Ok(updatedPolicy);
        }

        [HttpPut("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _policyService.CancelAsync(id);
            if (!success) return NotFound(new { message = "Apólice não encontrada." });
            return Ok(new { message = "Apólice cancelada com sucesso." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _policyService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Apólice não encontrada." });
            return Ok(new { message = "Apólice excluída com sucesso." });
        }
    }
}
