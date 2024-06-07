using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.Db;
using System.Net;

namespace Test.Dominio.Servicos
{
    [TestClass]
    public class VeiculoServicoTest
    {
        private DbContexto CriarContextoTeste()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            var config = builder.Build();
            return new DbContexto(config);
        }

        [TestMethod]
        public void TestarSalvarVeiculo()
        {
            //Arrange
            var _context = CriarContextoTeste();
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE veiculos");
            var veiculo = new Veiculo
            {
                Id = 1,
                Nome = "Carro",
                Marca = "Marca",
                Ano = 2001
            };

            //Act
            var veiculoServico = new VeiculoServico(_context);
            veiculoServico.Incluir(veiculo);

            //Assert
            Assert.AreEqual(veiculo, veiculoServico.BuscaPorId(1));
        }
    }
}
