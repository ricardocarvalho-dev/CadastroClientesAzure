namespace CadastroClientes.Application.DTOs;

public class ClienteDto
{
    public Guid Id { get; set; }
    public required string Nome { get; set; }
    public required string Celular { get; set; }
    public required string Email { get; set; }
    public DateTime DataCadastro { get; set; }
}
