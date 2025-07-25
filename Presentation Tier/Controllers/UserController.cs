﻿using Business_Tier;
using Interface_Tier.DTOs.User_DTOs;
using Interface_Tier.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Presentation_Tier.RequestDTOs;

namespace Presentation_Tier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService service) : ControllerBase
    {

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequestDTO dTO)
        {
          
            if (string.IsNullOrEmpty(dTO.Email) || string.IsNullOrEmpty(dTO.password))
            {
                return BadRequest("data is requierd.");
            }
            try

            {

                (string AccessToken, string RefreshToken) Result = await service.Login(dTO.Email, dTO.password); ;

                if (string.IsNullOrEmpty(Result.AccessToken) || string.IsNullOrEmpty(Result.RefreshToken))
                {
                    return Unauthorized();
                }



                return Ok(new { token = Result.AccessToken, refreshToken = Result.RefreshToken });
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost("GetAccessToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> GetAccessToken([FromBody] RefreshTokenRequestDTO dTO)
        {
            if (string.IsNullOrEmpty(dTO.RefreshToken))
            {
                return BadRequest("RefreshToken is requierd.");
            }

            try
            {

                string AccessToken = await service.GetAccessToken(dTO.RefreshToken); ;

                if (string.IsNullOrEmpty(AccessToken))
                {
                    return Unauthorized("Refresh token expired or revoked");
                }



                return Ok(new { token = AccessToken });
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost("AddNewUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddNewUser([FromForm] AddUserServiceDTO userServiceDTO)
        {
            if (userServiceDTO == null || userServiceDTO.image == null)
            {
                return BadRequest("data is requierd.");
            }

            try
            {
                int? newID = await service.AddUser(userServiceDTO); ;

                if (newID < 1)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while post.");
                }

                return NoContent();
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }


        [HttpPost("IsEmailVerifiedExists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsEmailVerifiedExists([FromBody] EmailCheckRequestDTO dTO)
        {
            if (string.IsNullOrEmpty(dTO.Email))
            {
                return BadRequest("Email is requierd.");
            }

            try
            {
                bool res = await service.IsEmailVerifiedExists(dTO.Email); ;

                return Ok(res);
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetSimpleUserData")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SimpleUserDataDTO>> GetSimpleUserData(int UserID)
        {
            if (UserID < 1)
            {
                return BadRequest("User ID is requierd.");
            }

            try
            {
                SimpleUserDataDTO res = await service.GetSimpleUserData(UserID); ;

                return Ok(res);
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDTO>> GetUser()
        {

            try
            {
                string userId = User.FindFirst("userID")?.Value;
                //string userId = "5";
                UserDTO res = await service.GetUser(int.Parse(userId));
                if (res == null)
                {
                    return NotFound();
                }
                return Ok(res);
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("IsPasswordMatched")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> IsPasswordMatched([FromBody] PasswordMatchRequestDTO dTO)
        {
            if (string.IsNullOrEmpty(dTO.Password))
            {
                return BadRequest("Password is requierd.");
            }

            try
            {
                string userId = User.FindFirst("userID")?.Value;
                //string userId = "5";
                bool res = await service.IsPasswordMatched(int.Parse(userId),dTO.Password); ;

                return Ok(res);
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("UpdateUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUser([FromForm] UpdateUserServiceRequestDTO userServiceDTO)
        {
            if (string.IsNullOrEmpty(userServiceDTO.Username) || string.IsNullOrEmpty(userServiceDTO.Email))
            {
                return BadRequest("data is requierd.");
            }

            try
            {
                string userId = User.FindFirst("userID")?.Value;
                //string userId = "5";
                UpdateUserServiceDTO dto = new UpdateUserServiceDTO
                {
                    UserID = int.Parse(userId),
                    image = userServiceDTO.image,
                    Username = userServiceDTO.Username,
                    Email = userServiceDTO.Email,
                };

                bool res = await service.UpdateUser(dto); ;

                if (!res)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while put.");
                }

                return NoContent();
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("UpdatePassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDTO dTO)
        {
            if (string.IsNullOrEmpty(dTO.oldPassword) || string.IsNullOrEmpty(dTO.newPassword))
            {
                return BadRequest("data is requierd.");
            }

            try
            {
                string userId = User.FindFirst("userID")?.Value;
                //string userId = "5";
              
                bool res = await service.UpdatePassword(int.Parse(userId),dTO.oldPassword,dTO.newPassword); ;

                if (!res)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
                }

                return NoContent();
            }
            catch (SqlException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }

    }
}
