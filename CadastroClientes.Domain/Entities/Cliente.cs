namespace CadastroClientes.Domain.Entities;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nome { get; set; }
    public required string Celular { get; set; }
    public required string Email { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
