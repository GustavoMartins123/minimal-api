using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Enums;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.ModelViews;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt")["Key"];

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataBase"))
    );

var app = builder.Build();


#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(key))
    {
        return string.Empty;
    }
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>() {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };
    var token = new JwtSecurityToken (
        claims : claims,
        expires : DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );
    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administrador/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.Login(loginDTO);
    if ( administrador != null)
    {
        string token = GerarTokenJwt(administrador);
        return Results.Ok(new AdministradorLogado
        {
            Email = administrador.Email,
            Perfil = administrador.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }

}).AllowAnonymous()
  .WithTags("Administrador");

app.MapGet("/administrador/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }
    var adm = new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    };
return Results.Ok(adm);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm"
  })
  .WithTags("Administrador");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm"
  })
  .WithTags("Administrador");

app.MapPost("/administrador", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new()
    };
    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("Email não pode ser vazio");
    }
    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("Senha não pode ser vazia");
    }
    if (administradorDTO.Perfil == null)
    {
        validacao.Mensagens.Add("Perfil não pode ser vazio");
    }
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm"
  })
  .WithTags("Administrador");
#endregion

#region Veiculos
ErrosDeValidacao ValidaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao {
        Mensagens = new()
    };
    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio");
    }
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ficar em branco");
    }
    if (veiculoDTO.Ano <= 1950)
    {
        validacao.Mensagens.Add("Veiculo muito antigo, somente anos superiores a 1950 serao aceitos");
    }
    return validacao;
}

app.MapPost("/veiculo/cadastrar", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = ValidaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}",veiculo);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm,Editor"
  })
  .WithTags("Veiculo");

app.MapGet("/veiculo/ProcurarTodos", ([FromQuery]int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm,Editor"
  })
  .WithTags("Veiculo");

app.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm,Editor"
  })
  .WithTags("Veiculo");

app.MapPut("/veiculo/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    var validacao = ValidaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm"
  })
  .WithTags("Veiculo");

app.MapDelete("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculoServico.Apagar(veiculo);
    return Results.NoContent();

}).RequireAuthorization()
  .RequireAuthorization(new AuthorizeAttribute
  {
      Roles = "Adm"
  })
  .WithTags("Veiculo");


#endregion

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
