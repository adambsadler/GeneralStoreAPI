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
    public class TransactionController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        [HttpPost]
        public async Task<IHttpActionResult> CreateTransaction([FromBody] Transaction model)
        {
            if (model is null)
                return BadRequest("Your request body cannot be empty.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var product = await _context.Products.FindAsync(model.ProductSKU);
            if (product is null)
                return BadRequest("That product does not exist.");

            if (!product.IsInStock)
                return BadRequest("That product is not in stock.");
           
            if (model.ItemCount > product.NumberInInventory)
                return BadRequest("There is not enough product in stock to complete this transaction.");

            _context.Transactions.Add(model);
            product.NumberInInventory -= model.ItemCount;
            await _context.SaveChangesAsync();
            return Ok("The transaction has been created.");
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactions()
        {
            List<Transaction> transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionById([FromUri] int id)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);

            if (transaction != null)
            {
                return Ok(transaction);
            }

            return NotFound();
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpdateTransaction([FromUri] int id, [FromBody] Transaction updatedTransaction)
        {
            if (id != updatedTransaction?.Id)
            {
                return BadRequest("These ids do not match.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Transaction transaction = await _context.Transactions.FindAsync(id);

            if (transaction is null)
                return NotFound();

            var product = await _context.Products.FindAsync(updatedTransaction.ProductSKU);
            if (product is null)
                return BadRequest("That product does not exist.");

            transaction.ItemCount = updatedTransaction.ItemCount;
            transaction.DateOfTransaction = updatedTransaction.DateOfTransaction;

            var updatedInventory = product.NumberInInventory + transaction.ItemCount;

            if (updatedTransaction.ItemCount > updatedInventory)
            {
                return BadRequest("There is not enough product in stock to update this transaction.");
            }

            product.NumberInInventory += transaction.ItemCount;
            product.NumberInInventory -= updatedTransaction.ItemCount;
            await _context.SaveChangesAsync();

            return Ok("The transaction information was updated.");
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTransaction([FromUri] int id)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);
            var product = await _context.Products.FindAsync(transaction.ProductSKU);
            if (product is null)
                return BadRequest("That product does not exist.");

            if (transaction is null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            product.NumberInInventory += transaction.ItemCount;

            if (await _context.SaveChangesAsync() == 1)
            {
                return Ok("The transaction was deleted.");
            }

            return InternalServerError();
        }
    }
}
