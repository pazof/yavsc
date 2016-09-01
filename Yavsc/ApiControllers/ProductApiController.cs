using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Market;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ProductApi")]
    public class ProductApiController : Controller
    {
        private ApplicationDbContext _context;

        public ProductApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductApi
        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            return _context.Products;
        }

        // GET: api/ProductApi/5
        [HttpGet("{id}", Name = "GetProduct")]
        public IActionResult GetProduct([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Product product = _context.Products.Single(m => m.Id == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return Ok(product);
        }

        // PUT: api/ProductApi/5
        [HttpPut("{id}"),Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult PutProduct(long id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/ProductApi
        [HttpPost,Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Products.Add(product);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/ProductApi/5
        [HttpDelete("{id}"),Authorize(Constants.FrontOfficeGroupName)]
        public IActionResult DeleteProduct(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Product product = _context.Products.Single(m => m.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Count(e => e.Id == id) > 0;
        }
    }
}