using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.ModelViews;
using System.Net;
using System.Text;
using System.Text.Json;
using Test.Helpers;

namespace Test.Requisições
{
    [TestClass]
    public class AdministradorRequestTest
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
    }
}
