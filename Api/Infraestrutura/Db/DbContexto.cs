using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;

namespace MinimalAPI.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;

        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Administrador> administradores { get; set; } = default!;
        public DbSet<Veiculo> veiculos { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuracaoAppSettings.GetConnectionString("DataBase"));
            }
        }
    }
}
