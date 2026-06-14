using CadastroClientes.Application.Interfaces;
using CadastroClientes.Domain.Entities;
using CadastroClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CadastroClientes.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<IEnumerable<Cliente>> ListarTodosAsync()
    {
        return await _context.Clientes
            .OrderByDescending(c => c.DataCadastro)
            .ToListAsync();
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> ObterPorEmailAsync(string email)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<bool> EmailJaExisteAsync(string email)
    {
        return await _context.Clientes.AnyAsync(c => c.Email == email);
    }
    public async Task<Cliente> AtualizarAsync(Cliente cliente)
{
    _context.Clientes.Update(cliente);
    await _context.SaveChangesAsync();
    return cliente;
}

public async Task ExcluirAsync(Cliente cliente)
{
    _context.Clientes.Remove(cliente);
    await _context.SaveChangesAsync();
}

}
