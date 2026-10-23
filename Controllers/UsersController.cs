using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

/// <summary>
/// Controller for managing user resources with full CRUD operations,
/// input validation, thread-safe in-memory storage, and appropriate HTTP status codes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    // Thread-safe in-memory store of users keyed by their unique integer ID
    private static readonly ConcurrentDictionary<int, User> _users = new();

    // Atomic counter for unique ID generation
    private static int _nextId = 0;

    // Lock object to guarantee thread safety during atomic checks (e.g. unique email checks)
    private static readonly object _syncLock = new();

    // Static constructor ensures sample data is seeded only once
    static UsersController()
    {
        SeedInitialUsers();
    }

    /// <summary>
    /// Seeds sample users for easy testing upon application startup.
    /// </summary>
    private static void SeedInitialUsers()
    {
        var sampleUsers = new List<User>
        {
            new() { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@example.com", Age = 28, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = 2, Name = "Bob Smith", Email = "bob.smith@example.com", Age = 34, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = 3, Name = "Charlie Brown", Email = "charlie.brown@example.com", Age = 22, CreatedAtUtc = DateTime.UtcNow }
        };

        foreach (var user in sampleUsers)
        {
            _users[user.Id] = user;
        }

        _nextId = sampleUsers.Max(u => u.Id);
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list of all registered users.</returns>
    /// <response code="200">Returns the list of all users.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<User>> GetAllUsers()
    {
        var users = _users.Values.OrderBy(u => u.Id).ToList();
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a single user by their unique ID.
    /// </summary>
    /// <param name="id">The integer ID of the user.</param>
    /// <returns>The matching user entity.</returns>
    /// <response code="200">User found successfully.</response>
    /// <response code="404">User with specified ID does not exist.</response>
    [HttpGet("{id:int}", Name = nameof(GetUserById))]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetUserById(int id)
    {
        if (!_users.TryGetValue(id, out var user))
        {
            return NotFound(new { message = $"User with ID {id} was not found." });
        }

        return Ok(user);
    }

    /// <summary>
    /// Creates a new user with server-generated ID and validated input.
    /// </summary>
    /// <param name="request">User creation details.</param>
    /// <returns>The created user and a location header.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="409">Email address already exists (case-insensitive).</response>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<User> CreateUser([FromBody] UserCreateRequest request)
    {
        lock (_syncLock)
        {
            // Case-insensitive email uniqueness check
            var emailExists = _users.Values.Any(u =>
                string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));

            if (emailExists)
            {
                return Conflict(new { message = $"A user with email '{request.Email}' already exists." });
            }

            var newId = Interlocked.Increment(ref _nextId);
            var newUser = new User
            {
                Id = newId,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                CreatedAtUtc = DateTime.UtcNow
            };

            _users[newId] = newUser;

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
    }

    /// <summary>
    /// Updates an existing user by ID.
    /// </summary>
    /// <param name="id">The integer ID of the user to update.</param>
    /// <param name="request">Updated user information.</param>
    /// <returns>The updated user.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="404">User with specified ID was not found.</response>
    /// <response code="409">Email address is already in use by another user.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<User> UpdateUser(int id, [FromBody] UserUpdateRequest request)
    {
        lock (_syncLock)
        {
            if (!_users.TryGetValue(id, out var existingUser))
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            // Ensure email is not in use by ANOTHER user (case-insensitive)
            var emailInUseByOther = _users.Values.Any(u =>
                u.Id != id && string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));

            if (emailInUseByOther)
            {
                return Conflict(new { message = $"Another user with email '{request.Email}' already exists." });
            }

            existingUser.Name = request.Name;
            existingUser.Email = request.Email;
            existingUser.Age = request.Age;

            return Ok(existingUser);
        }
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The integer ID of the user to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="404">User with specified ID was not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        lock (_syncLock)
        {
            if (!_users.TryRemove(id, out _))
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            return NoContent();
        }
    }
}
