using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ochakovo.Context;
using Ochakovo.Entities;

namespace Ochakovo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HrController : ControllerBase
    {
        private readonly OchakovoDbContext _context;

        public HrController(OchakovoDbContext context)
        {
            _context = context;
        }

        // GET: api/Hr
        [HttpGet]
        public async Task<ActionResult<IEnumerable<hr>>> Gethrs()
        {
            return await _context.hrs.ToListAsync();
        }

        // GET: api/Hr/5
        [HttpGet("{id}")]
        public async Task<ActionResult<hr>> Gethr(int id)
        {
            var hr = await _context.hrs.FindAsync(id);

            if (hr == null)
            {
                return NotFound();
            }

            return hr;
        }

        // PUT: api/Hr/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Puthr(int id, hr hr)
        {
            if (id != hr.HrId)
            {
                return BadRequest();
            }

            _context.Entry(hr).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!hrExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Hr
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<hr>> Posthr(hr hr)
        {
            _context.hrs.Add(hr);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Gethr", new { id = hr.HrId }, hr);
        }

        // DELETE: api/Hr/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Deletehr(int id)
        {
            var hr = await _context.hrs.FindAsync(id);
            if (hr == null)
            {
                return NotFound();
            }

            _context.hrs.Remove(hr);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool hrExists(int id)
        {
            return _context.hrs.Any(e => e.HrId == id);
        }
    }
}
