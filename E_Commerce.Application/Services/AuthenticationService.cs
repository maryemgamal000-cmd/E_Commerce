using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;

        public AuthenticationService(IIdentityService identityService , ITokenService tokenService)
        {
            _identityService = identityService;
            _tokenService = tokenService;
        }

        public async  Task<Result<bool>> CheckEmailExistsAsync(string email, CancellationToken ct = default)
        => await _identityService.EmailExistsAsync(email, ct);

        public async Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken ct = default)
        {
            var userResult = await _identityService.FindUserByEmailAsync(email, ct);

            var user = userResult.data;
            var roleResult = await _identityService.GetUserRoles(email, ct);

            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roleResult.data);
            return new UserDto() { DisplayName = user.DisplayName, Email = email, Token = token };
        }

        public Task<Result<AddressDto>> GetUserAddressAsync(string email, CancellationToken ct = default)
        {
            return _identityService.GetUserAddressByEmailAsync(email, ct);
        }

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            // Get User By Email
            var userResult = await _identityService.FindUserByEmailAsync(loginDto.Email);
            if (!userResult.IsSuccess)
            {
                return Result<UserDto>.Fail(userResult.Errors);
            }

            // Check Password
            var passwordResult = await _identityService.CheckPasswordAsync(loginDto.Email, loginDto.Password);
            if (!passwordResult.IsSuccess)
            {
                return Result<UserDto>.Fail(userResult.Errors);
            }
            if (!passwordResult.data)
            {
                return Result<UserDto>.Fail(Error.Unauthorized("Invalid Email Or Password"));
            }


            var user = userResult.data;
            var rolesResult = await _identityService.GetUserRoles(user.Email);
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);
            return new UserDto
            {
                Email = userResult.data.Email,
                DisplayName = userResult.data.DisplayName,
                Token = token
            };
        }

        public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default)
        {
            var userResult = await _identityService.CreateUserAsync(registerDto, ct);

            if (!userResult.IsSuccess)
            {
                return Result<UserDto>.Fail(userResult.Errors);
            }

            var user = userResult.data;
            var rolesResult = await _identityService.GetUserRoles(user.Email);
            if (!rolesResult.IsSuccess)
            {
                return Result<UserDto>.Fail(rolesResult.Errors);
            }
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return Result<UserDto>.Ok(new UserDto()
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = token
            });
        }

        public async Task<Result<AddressDto>> UpSertUserAddressAsync(string email, AddressDto addressDto, CancellationToken ct = default)
        {
            return await _identityService.UpdateOrInsertUserAddressAsync(email, addressDto, ct);
        }
    }
}
