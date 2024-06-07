using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;

namespace Test.Mocks
{
    public class VeiculoServicoMock : IVeiculoServico
    {
        private static readonly List<Veiculo> veiculos = new List<Veiculo>();

        public void Apagar(Veiculo veiculo)
        {
            if(veiculo == null)
            {
                throw new Exception("Veiculo nâo encontrado");
            }
            veiculos.Remove(veiculo);
        }

        public void Atualizar(Veiculo veiculo)
        {
            var novoVeiculo = veiculos.Find(x => x.Id == veiculo.Id) ?? throw new Exception("Veiculo não encontrado");
            novoVeiculo.Nome = veiculo.Nome;
            novoVeiculo.Marca = veiculo.Marca;
            novoVeiculo.Ano = veiculo.Ano;
        }

        public Veiculo? BuscaPorId(int id)
        {
            return veiculos.Find(x => x.Id == id);
        }

        public void Incluir(Veiculo veiculo)
        {
            veiculo.Id = veiculos.Count + 1;
            veiculos.Add(veiculo);
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = veiculos.AsQueryable();
            if (!string.IsNullOrEmpty(nome))
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
    }
}
