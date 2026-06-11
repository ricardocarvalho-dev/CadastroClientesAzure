using CadastroClientes.Application.DTOs;
using CadastroClientes.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly CriarClienteUseCase _criarUseCase;
    private readonly ListarClientesUseCase _listarUseCase;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        CriarClienteUseCase criarUseCase,
        ListarClientesUseCase listarUseCase,
        ILogger<ClientesController> logger)
    {
        _criarUseCase = criarUseCase;
        _listarUseCase = listarUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    /// <param name="dto">Dados do cliente</param>
    /// <returns>Cliente criado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteDto dto)
    {
        try
        {
            var cliente = await _criarUseCase.Executar(dto);
            return Ok(new { mensagem = "Cliente criado com sucesso", cliente });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Erro de negócio ao criar cliente: {ex.Message}");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar cliente: {ex.Message}");
            return StatusCode(500, new { 
                mensagem = "Erro interno ao criar cliente",
                erroReal = ex.Message,
                erroInterno = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Lista todos os clientes cadastrados
    /// </summary>
    /// <returns>Lista de clientes</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var clientes = await _listarUseCase.Executar();
            return Ok(new { mensagem = "Clientes listados com sucesso", clientes });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao listar clientes: {ex.Message}");
            return StatusCode(500, new { 
                mensagem = "Erro interno ao listar clientes",
                erroReal = ex.Message
            });
        }
    }
}
