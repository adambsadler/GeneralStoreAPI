using GeneralStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeneralStoreAPI.Controllers
{
    public class CustomerController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        [HttpPost]
        public async Task<IHttpActionResult> CreateCustomer([FromBody] Customer model)
        {
            if (model is null)
            {
                return BadRequest("Your request body cannot be empty.");
            }

            if (ModelState.IsValid)
            {
                _context.Customers.Add(model);
                await _context.SaveChangesAsync();
                return Ok("The customer has been created.");
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllCustomers()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCustomerById([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                return Ok(customer);
            }

            return NotFound();
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpdateCustomer([FromUri] int id, [FromBody] Customer updatedCustomer)
        {
            if(id != updatedCustomer?.Id)
            {
                return BadRequest("These ids do not match.");
            }

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            Customer customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound();

            customer.FirstName = updatedCustomer.FirstName;
            customer.LastName = updatedCustomer.LastName;

            await _context.SaveChangesAsync();

            return Ok("The customer information was updated.");
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCustomer([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer is null)
                return NotFound();

            _context.Customers.Remove(customer);

            if(await _context.SaveChangesAsync() == 1)
            {
                return Ok("The customer was deleted.");
            }

            return InternalServerError();
        }
    }
}
