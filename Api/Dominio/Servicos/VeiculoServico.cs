using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Infraestrutura.Db;

namespace MinimalAPI.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DbContexto _contexto;
        public VeiculoServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _contexto.veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(x => EF.Functions.Like(x.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }
            int itensPorPagina = 10;
            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }
            return query.ToList();
        }

        public Veiculo? BuscaPorId(int id)
        {
            var veiculo = _contexto.veiculos.Find(id);
            return veiculo;
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public void Apagar(Veiculo veiculo)
        {
            if(veiculo == null)
            {
                throw new Exception("Veiculo não encontrado");
            }
            _contexto.veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }
    }
}
