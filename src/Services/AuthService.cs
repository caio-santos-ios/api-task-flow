using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using to_do_list.src.Helpers;
using to_do_list.src.Interfaces;
using to_do_list.src.Models;
using to_do_list.src.Models.Base;
using to_do_list.src.Requests;
using to_do_list.src.Shared.Utils;

namespace to_do_list.src.Services
{
    public class AuthService(
        IUserRepository userRepository,
        MailHelper mailHelper
    ) : IAuthService
    {
        #region LOGIN
        public async Task<ResponseApi<dynamic?>> LoginAsync(LoginRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByEmailAsync(request.Email);
                if (response.Data is null) return new(null, 400, "E-mail ou senha são incorretos");

                if (response.Data.Blocked) return new(null, 400, "Conta bloqueada, entre em contato com o Administrador do TaskFlow");
                
                if (!response.Data.ValidatedAccess)
                {
                    dynamic generateCode = Util.GenerateCodeAccess();

                    response.Data.CodeAccess = generateCode.CodeAccess;
                    response.Data.CodeAccessExpiration = generateCode.CodeAccessExpiration;

                    await userRepository.UpdateAsync(response.Data);

                    await mailHelper.SendMail(request.Email, "Código de Confirmação", $"Seu código de confirmação: {generateCode.CodeAccess}");

                    return new(null, 400, "Conta não foi confirmada, enviamos um e-mail de confirmação novamente");
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, response.Data.Password);
                if (!isValid) return new(null, 400, "Dados incorretos");

                return new(new { Token = GenerateJwtToken(response.Data), response.Data.Photo, response.Data.Name, admin = response.Data.Admin.ToString() }, 200, "Login feito com sucesso.");
            }
            catch
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde");
            }
        }
        #endregion

        #region RESET PASSWORD
        public async Task<ResponseApi<dynamic?>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByEmailAsync(request.Email);
                if (response.Data is null) return new(null, 400, response.Message);

                dynamic generateCode = Util.GenerateCodeAccess();

                response.Data.CodeAccess = generateCode.CodeAccess;
                response.Data.CodeAccessExpiration = generateCode.CodeAccessExpiration;

                await userRepository.UpdateAsync(response.Data);

                var res = await mailHelper.SendMail(response.Data.Email, "Código de Verificação", $"Seu código de veficação: {generateCode.CodeAccess}");

                return new(new { }, 200, "Foi enviado um código de verificação para o e-mail.");
            }
            catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        public async Task<ResponseApi<dynamic?>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByCodeAsync(request.Code);
                if (response.Data is null) return new(null, 400, "Código inválido");

                DateTime today = DateTime.Now;

                if(response.Data.CodeAccessExpiration > today) return new(null, 400, "Código expirou, deve solicitar outro");

                dynamic generateCode = Util.GenerateCodeAccess();

                response.Data.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);;

                await userRepository.UpdateAsync(response.Data);

                return new(new { }, 200, "Senha resetada com sucesso.");
            }
            catch(Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion
        public string GenerateJwtToken(User user, bool refresh = false)
        {
            string SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? "";
            string Issuer = Environment.GetEnvironmentVariable("ISSUER") ?? "";
            string Audience = Environment.GetEnvironmentVariable("AUDIENCE") ?? "";

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(SecretKey));

            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("type", refresh ? "refresh" : "access"),
                new Claim("name", user.Name),
                new Claim("photo", user.Photo),
                new Claim("admin", user.Admin.ToString()),
            ];

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: refresh ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}