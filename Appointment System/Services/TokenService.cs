using Azure.Identity;
using Appointment_System.Data;
using Appointment_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;

namespace Appointment_System.Services
{
    public class TokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public TokenService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IdentityResult> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return IdentityResult.Failed(new IdentityError { Code = "EmptyToken", Description = "Token cannot be empty" });
            }
            
            var record = await _context.Tokens.FirstOrDefaultAsync(t => t.AccessToken == token);
            if (record == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token" });
            }
            if (record.ExpiresOn < DateTimeOffset.UtcNow)
            {
                return IdentityResult.Failed(new IdentityError { Code = "ExpiredToken", Description = "Token has expired" });
            }
            return IdentityResult.Success;
        }

        public async Task<string> GetTokenAsync()
        {
            var record = await _context.Tokens.FirstOrDefaultAsync();
            if (record != null && record.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return record.AccessToken;
            }

            // 获取新 token
            var tenantId = _config["AzureAD:TenantId"];
            var clientId = _config["AzureAD:ClientId"];
            var clientSecret = _config["AzureAD:ClientSecret"];
            var scope = _config["AzureAD:Scope"];

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { scope }));

            if (record == null)
            {
                record = new TokenRecord();
                _context.Tokens.Add(record);
            }

            record.AccessToken = token.Token;
            record.ExpiresOn = token.ExpiresOn;

            await _context.SaveChangesAsync();

            return token.Token;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = rememberMe ? DateTime.Now.AddDays(15) : DateTime.Now.AddHours(Convert.ToDouble(_config["Jwt:ExpireHours"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task BlacklistToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be empty", nameof(token));
            }
            
            // Check if token exists
            var existingToken = await _context.Tokens.FindAsync(token);
            
            // If token exists, remove it (blacklist it)
            if (existingToken != null)
            {
                _context.Tokens.Remove(existingToken);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Add token to blacklist with minimal information
                var blacklistedToken = new TokenRecord
                {
                    AccessToken = token,
                    ExpiresOn = DateTimeOffset.UtcNow.AddYears(1) // Set far future expiration for blacklisted tokens
                };

                _context.Tokens.Add(blacklistedToken);
                await _context.SaveChangesAsync();
            }
        }
    }
}
