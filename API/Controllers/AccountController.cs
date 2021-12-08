using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController  : BaseAPIController
    {
        public DataContext _context { get; }
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context , ITokenService tokenService )
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(ResigterDto registerDto )
        {
            if(await UserExists(registerDto.UserName))
            {
                return BadRequest("User is taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hmac.Key

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            return new UserDto{ Username = user.UserName , Token = _tokenService.CreatToken(user) };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDto )
        {
            var user = await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(u=>u.UserName == loginDto.Username.ToLower());
            if(user == null) return Unauthorized("Bad username or password") ;
            
             using var hmac = new HMACSHA512(user.PasswordSalt);
             var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
             for(int i=0 ; i< computedHash.Length ; i++)
             {
                 if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
             }
             return new UserDto{ 
                 Username = user.UserName , 
                 Token = _tokenService.CreatToken(user),
                 PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url };

        }
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName == username.ToLower());
        }
    }
}