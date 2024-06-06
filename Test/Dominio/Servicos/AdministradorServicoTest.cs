using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.Db;

namespace Test.Dominio.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
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
        public void TestarSalvarAdministrador()
        {
            //Arrange
            var _context = CriarContextoTeste();
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");
            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            //Act
            var administradorServico = new AdministradorServico(_context);
            administradorServico.Incluir(adm);

            //Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestarBuscaPorId()
        {
            //Arrange
            var _context = CriarContextoTeste();
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");
            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "Adm";

            //Act
            var administradorServico = new AdministradorServico(_context);
            administradorServico.Incluir(adm);
            var administrador = administradorServico.BuscarPorId(adm.Id);

            //Assert
            Assert.AreEqual(1, administrador?.Id);
        }
    }
}
