using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.ModelViews;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Test.Helpers;

namespace Test.Requisições
{
    [TestClass]
    public class AdministradorEVeiculoRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Setup.ClassInit(context);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }
        #region Administrador
        [TestMethod]
        public async Task TestarGetSetPropriedades()
        {
            //Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");
            //Act
            var response = await Setup.client.PostAsync("/administrador/login", content);
            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.IsNotNull(admLogado?.Email);
            Assert.IsNotNull(admLogado?.Perfil);
            Assert.IsNotNull(admLogado.Token);
        }
        #endregion

        #region Veiculo
        [TestMethod]
        public async Task TestarBuscarECompararVeiculo()
        {
            //Arrange
            var veiculo = new VeiculoDTO
            {
                Nome = "Marea Turbo",
                Marca = "Ford",
                Ano = 2001
            };
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");
            //Act
            var response = await Setup.client.PostAsync("/administrador/login", content);
            //Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var token = admLogado.Token;
                Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var getResponse = await Setup.client.GetAsync("/veiculo/ProcurarTodos");
                Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

                var resultVeiculos = await getResponse.Content.ReadAsStringAsync();
                var veiculosBuscados = JsonSerializer.Deserialize<List<VeiculoDTO>>(resultVeiculos, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.IsNotNull(veiculosBuscados);
                Assert.AreNotEqual(veiculo, veiculosBuscados.FirstOrDefault());

                var veiculoEncontrado = veiculosBuscados.Any(v =>
                    v.Nome == veiculo.Nome &&
                    v.Marca == veiculo.Marca &&
                    v.Ano == veiculo.Ano
                );

                Assert.IsFalse(veiculoEncontrado);
            }
        }
        #endregion
    }
}
