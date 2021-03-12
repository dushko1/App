using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenInterface tokenInterface;
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenInterface tokenInterface, IMapper mapper)
        {
            this.mapper = mapper;
            this.tokenInterface = tokenInterface;
            this.context = context;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {

            if (await UserExists(dto.Username))
            {
                return BadRequest("Username is taken");
            }
            var user = mapper.Map<AppUser>(dto);

            using var hmac = new HMACSHA512();

            user.UserName = dto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            user.PasswordSalt = hmac.Key;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = tokenInterface.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };

        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await context.Users.Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == dto.Username);
            if (user == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = tokenInterface.CreateToken(user),
                photoUrl = user.Photos.FirstOrDefault(x => x.isMain)?.url,
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };

        }
        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName == username.ToLower());

        }

    }
}
