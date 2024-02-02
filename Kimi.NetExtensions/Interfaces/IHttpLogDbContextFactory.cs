using Microsoft.EntityFrameworkCore;


public interface IHttpLogDbContextFactory
{
    // Define a method to create a DbContext
    DbContext CreateDbContext();
}