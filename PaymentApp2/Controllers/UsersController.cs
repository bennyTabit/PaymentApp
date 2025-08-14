using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentApp.Data;
using PaymentApp.Models;
using PaymentApp.Models.DTOs;

namespace PaymentApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly PaymentDbContext _context;

    public UsersController(PaymentDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users.Select(u => new UserResponseDto { Id = u.Id, Name = u.Name, Email = u.Email }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email });
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser(CreateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = new User { Name = dto.Name, Email = dto.Email };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var response = new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email };
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (dto.Name != null) user.Name = dto.Name;
        if (dto.Email != null) user.Email = dto.Email;
        await _context.SaveChangesAsync();
        return Ok(new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
