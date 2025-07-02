using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Appointment.System.Tests
{
    // Example entity
    public class ExampleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    // Example DbContext
    public class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExampleEntity> Entities { get; set; }
    }

    // Example repository
    public class ExampleRepository
    {
        private readonly ExampleDbContext _context;

        public ExampleRepository(ExampleDbContext context)
        {
            _context = context;
        }

        public async Task<ExampleEntity> GetByIdAsync(int id)
        {
            return await _context.Entities.FindAsync(id);
        }

        public async Task<ExampleEntity[]> GetActiveEntitiesAsync()
        {
            return await _context.Entities
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToArrayAsync();
        }

        public async Task AddEntityAsync(ExampleEntity entity)
        {
            await _context.Entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEntityAsync(ExampleEntity entity)
        {
            _context.Entities.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEntityAsync(int id)
        {
            var entity = await _context.Entities.FindAsync(id);
            if (entity != null)
            {
                _context.Entities.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    // Test class
    public class DatabaseExampleTest
    {
        private ExampleDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ExampleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ExampleDbContext(options);
            
            // Seed the database
            context.Entities.AddRange(
                new ExampleEntity { Id = 1, Name = "Entity 1", IsActive = true },
                new ExampleEntity { Id = 2, Name = "Entity 2", IsActive = false },
                new ExampleEntity { Id = 3, Name = "Entity 3", IsActive = true }
            );
            context.SaveChanges();
            
            return context;
        }

        [Fact]
        public async Task GetActiveEntities_ReturnsOnlyActiveEntities()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ExampleRepository(context);

            // Act
            var result = await repository.GetActiveEntitiesAsync();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal("Entity 1", result[0].Name);
            Assert.Equal("Entity 3", result[1].Name);
            Assert.All(result, entity => Assert.True(entity.IsActive));
        }

        [Fact]
        public async Task AddEntity_AddsEntityToDatabase()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ExampleRepository(context);
            var newEntity = new ExampleEntity { Id = 4, Name = "Entity 4", IsActive = true };

            // Act
            await repository.AddEntityAsync(newEntity);

            // Assert
            var entity = await context.Entities.FindAsync(4);
            Assert.NotNull(entity);
            Assert.Equal("Entity 4", entity.Name);
            Assert.True(entity.IsActive);
        }

        [Fact]
        public async Task UpdateEntity_UpdatesEntityInDatabase()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ExampleRepository(context);
            
            // Get existing entity and modify it
            var entity = await context.Entities.FindAsync(1);
            entity.Name = "Updated Entity";
            entity.IsActive = false;

            // Act
            await repository.UpdateEntityAsync(entity);

            // Assert
            var updatedEntity = await context.Entities.FindAsync(1);
            Assert.Equal("Updated Entity", updatedEntity.Name);
            Assert.False(updatedEntity.IsActive);
        }

        [Fact]
        public async Task DeleteEntity_RemovesEntityFromDatabase()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ExampleRepository(context);

            // Act
            await repository.DeleteEntityAsync(1);

            // Assert
            var deletedEntity = await context.Entities.FindAsync(1);
            Assert.Null(deletedEntity);
            
            // Verify other entities still exist
            Assert.NotNull(await context.Entities.FindAsync(2));
            Assert.NotNull(await context.Entities.FindAsync(3));
        }
    }
} 