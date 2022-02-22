using AutoMapper;
using Catalog.API.Dtos.Request;
using Catalog.API.Dtos.Response;
using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [Produces("application/json")]
    [EnableCors("AllowOrigin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;
        private readonly IMapper _mapper;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger,
            IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }
        
        /// <summary>
        /// Gets a list of product catalog
        /// </summary>
        /// <returns>Returns a list of products</returns>
        /// <returns code="200">Returns a list of product catalogs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _repository.GetProducts();
            return Ok(new ApiResponse<IEnumerable<Product>>
            {
                Status = true,
                Data = products,
                Message = null
            });
        }

        /// <summary>
        /// Gets a product by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a product</returns>
        /// <returns code="400">Returns product not found</returns>
        /// <returns code="200">Gets a Single Product</returns>
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _repository.GetProduct(id);
            if (product == null)
            {
                _logger.LogError($"Product with id: {id}, not found");
                return NotFound(new ApiResponse<Product>
                {
                    Status = false,
                    Data = null,
                    Message = $"Product with id: {id}, not found"
                });
            }

            return Ok(new ApiResponse<Product>
            {
                Status = true,
                Data = product,
                Message = $"Product data found"
            });
        }

        /// <summary>
        /// Gets a list of products by category name
        /// </summary>
        /// <returns>Returns a product list</returns>
        /// <returns code="200">Returns a product list</returns>
        [HttpGet, Route("[action]/{category}", Name = "GetProductByCategory")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProductByCategory(string category)
        {
            var products = await _repository.GetProductByCategory(category);
            return Ok(new ApiResponse<IEnumerable<Product>>
            {
                Status = true,
                Message = null,
                Data = products
            });
        }

        /// <summary>
        /// Creates a new Product
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns>Created product</returns>
        /// <returns code="200">A created product with a product id</returns>
        /// <returns code="422">Cannot create product, product exists</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.UnprocessableEntity)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            // check product exists
            var product = _mapper.Map<Product>(productDto);

            if (await _repository.CheckProductExists(name: product.Name))
            {
                return UnprocessableEntity(new ApiResponse<Product>
                {
                    Status = false,
                    Message = $"Product {product.Name} already exists",
                    Data = null
                });
            }


            await _repository.CreateProduct(product);

            // return CreatedAtRoute("GetProduct", new { id == product.Id }, product);

            return StatusCode(StatusCodes.Status201Created, new ApiResponse<Product>
            {
                Status = true,
                Data = product,
                Message = $"Product successfully created"
            });
        }

        /// <summary>
        /// Updates a specific product
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns>Update successful</returns>
        /// <returns code="200">Product updated successfully</returns>
        /// <returns code="404">Product not found</returns>
        /// <returns code="500">Internal server error</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductDto productDto)
        {

            // check product exists
            var product = _mapper.Map<Product>(productDto);

            if (!await _repository.CheckProductExists(name: productDto.Name))
            {
                return NotFound(new ApiResponse<Product>
                {
                    Status = false,
                    Message = $"Product {product.Name} does not exist",
                    Data = null
                });
            }

            var updated = await _repository.UpdateProduct(product);
            if (!updated)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Product> { 
                    Status = false,
                    Data = null,
                    Message = $"Error updating product {product.Name}, please try again later"
                });
            }

            return Ok(new ApiResponse<Product>
            {
                Status = true,
                Data = null,
                Message = $"Product {product.Name} updated successfully!"
            });
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns>Product deletion successful</returns>
        /// <returns code="200">Product deleted successfully</returns>
        /// <returns code="404">Product not found</returns>
        /// <returns code="500">Internal server error</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Product>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _repository.GetProduct(id: id);

            if (product == null)
            {
                return NotFound(new ApiResponse<Product> {
                    Status = false,
                    Message = $"Product, {product.Id} not found",
                    Data = null
                });
            }

            var isDeleted = await _repository.DeleteProduct(id);

            if (!isDeleted)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Product>
                {
                    Status = false,
                    Data = null,
                    Message = $"Error deleting product {product.Name}, please try again later"
                });
            }

            return Ok(new ApiResponse<Product>
            {
                Status = true,
                Data = null,
                Message = $"Product {product.Name} deleted successfully!"
            });
        }
    }
}
