using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCaching.Data;
using RedisCaching.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RedisCacheService _redis;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ApplicationDbContext context,
        RedisCacheService redis,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _redis = redis;
        _logger = logger;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _redis.GetOrSetAsync("GET_ALL_PRODUCTS",
                async () => await _context.Products.ToListAsync());

            return Ok(products ?? new List<Product>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all products");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _redis.GetOrSetAsync($"Product_{id}",
                async () => await _context.Products.FindAsync(id));

            return product == null
                ? NotFound($"Product {id} not found")
                : Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Invalidate the all products cache
            await _redis.RemoveAsync("GET_ALL_PRODUCTS");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        try
        {
            if (id != product.Id)
                return BadRequest("ID mismatch");

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Invalidate both individual and all products cache
            await Task.WhenAll(
                _redis.RemoveAsync($"Product_{id}"),
                _redis.RemoveAsync("GET_ALL_PRODUCTS")
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Invalidate both individual and all products cache
            await Task.WhenAll(
                _redis.RemoveAsync($"Product_{id}"),
                _redis.RemoveAsync("GET_ALL_PRODUCTS")
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}