using CadastroClientes.Application.DTOs;
using CadastroClientes.Application.Interfaces;
using Microsoft.Extensions.Logging;
using CadastroClientes.Application.Validators;
using CadastroClientes.Domain.Entities;
using FluentValidation;

namespace CadastroClientes.Application.UseCases;

public class CriarClienteUseCase
{
    private readonly IClienteRepository _repository;
    private readonly IMessagingService _messagingService;
    private readonly CriarClienteDtoValidator _validator;
    private readonly ILogger<CriarClienteUseCase> _logger;

    public CriarClienteUseCase(
        IClienteRepository repository,
        IMessagingService messagingService,
        ILogger<CriarClienteUseCase> logger)
    {
        _repository = repository;
        _messagingService = messagingService;
        _validator = new CriarClienteDtoValidator();
        _logger = logger;
    }

    public async Task<ClienteDto> Executar(CriarClienteDto dto)
    {
        try
        {
            // Valida o DTO
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }

            // Verifica se email já existe
            bool emailJaExiste = await _repository.EmailJaExisteAsync(dto.Email);
            if (emailJaExiste)
            {
                throw new InvalidOperationException($"Email {dto.Email} já está cadastrado");
            }

            // Cria o cliente
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                Celular = dto.Celular,
                DataCadastro = DateTime.UtcNow
            };

            var clienteCriado = await _repository.CriarAsync(cliente);
            _logger.LogInformation($"Cliente criado com ID: {clienteCriado.Id}");

            // Publica para a fila RabbitMQ
            await _messagingService.PublicarCriacaoClienteAsync(
                clienteCriado.Id,
                clienteCriado.Nome,
                clienteCriado.Email,
                clienteCriado.Celular);

            return new ClienteDto
            {
                Id = clienteCriado.Id,
                Nome = clienteCriado.Nome,
                Email = clienteCriado.Email,
                Celular = clienteCriado.Celular,
                DataCadastro = clienteCriado.DataCadastro
            };
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning($"Erro de validação: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar cliente: {ex.Message}");
            throw;
        }
    }
}
