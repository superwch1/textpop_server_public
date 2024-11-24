using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MessageWebServer.Services;
using MessageWebServer.Models.Account;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MessageWebServer.Repository;
using textpop_server.Services.LoginProviderValidation;
using textpop_server.Services.Image;
using textpop_server.Services;

namespace MessageWebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<TextpopAppUser> _siginManager;
        private readonly UserManager<TextpopAppUser> _userManager;
        private readonly AccountRepository _accountRepository;
        private readonly TextpopJwtToken _jwtToken;
        private readonly UploadImage _uploadImage;
        private readonly ScanImage _scanImage;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public AccountController(SignInManager<TextpopAppUser> signInManager, UserManager<TextpopAppUser> userManager, AccountRepository accountRepository, TextpopJwtToken jwtToken, 
            UploadImage uploadImage, ScanImage scanImage, IWebHostEnvironment webHostEnvironment)
        {
            _siginManager = signInManager;
            _userManager = userManager;
            _accountRepository = accountRepository;
            _jwtToken = jwtToken;
            _uploadImage = uploadImage;
            _scanImage = scanImage;
            _webHostEnvironment = webHostEnvironment;
        }


        /// <summary>
        /// Validate the google token with response 201 (user created) or 202 (user found)
        /// </summary>
        /// <param name="googleToken"></param>
        /// <returns>appToken, userId and username (201 or 202)</returns>
        [HttpGet]
        public async Task<IActionResult> LoginWithGoogleToken(string googleToken, string fcmToken)
        {
            try
            {
                var googleUser = await GoogleJsonWebSignature.ValidateAsync(googleToken);

                var validateResult = await _siginManager.ExternalLoginSignInAsync("Google", googleUser.Subject, false);
                if (validateResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(googleUser.Email);
                    string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);

                    //delete original token and create new one if same token name but different user Id
                    await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                    return Accepted(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                }

                else
                {
                    var count = 1;
                    while (true)
                    {
                        var matchedUser = await _userManager.FindByNameAsync($"Acc{_userManager.Users.Count() + count}");
                        if (matchedUser == null)
                        {
                            break;
                        }
                        count++;
                    }

                    var newUser = new TextpopAppUser { UserName = $"Acc{_userManager.Users.Count() + count}", Email = googleUser.Email };
                    var createUserResult = await _userManager.CreateAsync(newUser);

                    if (createUserResult.Succeeded)
                    {
                        await _userManager.AddLoginAsync(newUser, new UserLoginInfo("Google", googleUser.Subject, "Google"));
                        
                        var user = await _userManager.FindByEmailAsync(googleUser.Email);

                        //delete original token and create new one if same token name but different user Id
                        await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                        string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);
                        _uploadImage.SaveDefaultImageForNewUser(user.Id);

                        return Created(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                    }
                }
                return BadRequest();
            }
            catch
            {           
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Validate the apple token with response 201 (user created) or 202 (user found)
        /// </summary>
        /// <param name="appleToken"></param>
        /// <returns>appToken, userId and username (201 or 202)</returns>
        [HttpGet]
        public async Task<IActionResult> LoginWithAppleToken(string appleToken, string fcmToken)
        {
            try
            {
                AppleValidation appleValidation = new AppleValidation();
                Tuple<bool, string, string> validationResult = await appleValidation.ValidateAppleTokenAndGetInfo(appleToken);

                if (validationResult.Item1 == false)
                {
                    return BadRequest();
                }

                string appleUserId = validationResult.Item2;
                string appleUserEmail = validationResult.Item3;

                var validateResult = await _siginManager.ExternalLoginSignInAsync("Apple", appleUserId, false);
                if (validateResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(appleUserEmail);
                    string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);

                    //delete original token and create new one if same token name but different user Id
                    await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                    return Accepted(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                }

                else
                {
                    var count = 1;
                    while (true)
                    {
                        var matchedUser = await _userManager.FindByNameAsync($"Acc{_userManager.Users.Count() + count}");
                        if (matchedUser == null)
                        {
                            break;
                        }
                        count++;
                    }

                    var newUser = new TextpopAppUser { UserName = $"Acc{_userManager.Users.Count() + count}", Email = appleUserEmail };
                    var createUserResult = await _userManager.CreateAsync(newUser);

                    if (createUserResult.Succeeded)
                    {
                        var addLoginMethodResult = await _userManager.AddLoginAsync(newUser, new UserLoginInfo("Apple", appleUserId, "Apple"));

                        var user = await _userManager.FindByEmailAsync(appleUserEmail);

                        //delete original token and create new one if same token name but different user Id
                        await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                        string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);
                        _uploadImage.SaveDefaultImageForNewUser(user.Id);

                        return Created(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                    }
                }

                return BadRequest();
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Validate the facebook token with response 201 (user created) or 202 (user found)
        /// </summary>
        /// <param name="facebookToken"></param>
        /// <returns>appToken, userId and username (201 or 202)</returns>
        [HttpGet]
        public async Task<IActionResult> LoginWithFacebookToken(string facebookToken, string fcmToken)
        {
            try
            {
                FacebookValidation facebookValidation = new FacebookValidation();
                Tuple<bool, string?, string?> validationResult = await facebookValidation.ValidateFacebookTokenAndGetInfo(facebookToken);

                if (validationResult.Item1 == false)
                {
                    return BadRequest();
                }

                string facebookUserId = validationResult.Item2!;
                string facebookUserEmail = validationResult.Item3!;

                var validateResult = await _siginManager.ExternalLoginSignInAsync("Facebook", facebookUserId, false);
                if (validateResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(facebookUserEmail);
                    string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);

                    //delete original token and create new one if same token name but different user Id
                    await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                    return Accepted(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                }

                else
                {
                    var count = 1;
                    while (true)
                    {
                        var matchedUser = await _userManager.FindByNameAsync($"Acc{_userManager.Users.Count() + count}");
                        if (matchedUser == null)
                        {
                            break;
                        }
                        count++;
                    }

                    var newUser = new TextpopAppUser { UserName = $"Acc{_userManager.Users.Count() + count}", Email = facebookUserEmail };
                    var createUserResult = await _userManager.CreateAsync(newUser);

                    if (createUserResult.Succeeded)
                    {
                        var addLoginMethodResult = await _userManager.AddLoginAsync(newUser, new UserLoginInfo("Facebook", facebookUserId, "Facebook"));

                        var user = await _userManager.FindByEmailAsync(facebookUserEmail);

                        //delete original token and create new one if same token name but different user Id
                        await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                        string appToken = _jwtToken.CreateToken(user.Id, user.UserName, user.Email);
                        _uploadImage.SaveDefaultImageForNewUser(user.Id);

                        return Created(user.Id, new { appToken = appToken, userId = user.Id, username = user.UserName });
                    }
                }

                return BadRequest();
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Validate the apptoken then update the avatar and username 
        /// </summary>
        /// <param name="uploadImage"></param>
        /// <param name="username"></param>
        /// <returns>userId (201)</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUserInfo([FromForm] IFormFile? image, [FromForm] string? username)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest();
                }


                if (username != null)
                {
                    Regex regex = new Regex("^[A-Za-z0-9><@_.-]{4,10}$");
                    if (!regex.IsMatch(username))
                    {
                        return BadRequest();
                    }
                    user.UserName = username;
                }


                if (image != null)
                {
                    byte[] uploadImageInByte;
                    Tuple<bool, string?, long> uploadResult;
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        uploadImageInByte = memoryStream.ToArray();

                        var isNotImage = _scanImage.IsNotImage(uploadImageInByte);
                        var containVirus = false; // await _scanImage.ContainVirus(imageInByte);

                        if (isNotImage || containVirus)
                        {
                            return BadRequest();
                        }
                        uploadResult = await _uploadImage.UploadResizeAvatarAndReturnInfo(uploadImageInByte, user.Id);
                        user.AvatarUri = uploadResult.Item2;
                    }

                    if (uploadResult.Item1 == false)
                    {
                        return BadRequest();
                    }
                }


                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Created(user.Id, new { });
                }

                return BadRequest();
            }
            catch 
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Show the user avatar via link
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>avatar uploadImage in jpg(200)</returns>
        [HttpGet]
        public async Task<IActionResult> ReadUserAvatar(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest();
                }
                return File(@$"Image\Account\{userId}.jpg", "uploadImage/jpeg");

            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Validate the token then get user info
        /// </summary>
        /// <returns>username and userId (200)</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserLogin(string fcmToken)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest();
                }

                //delete original token and create new one if same token name but different user Id
                await _accountRepository.CreateUserFCMToken(user.Id, fcmToken);

                return Ok(new { userId = user.Id, username = user.UserName });
            }
            catch 
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Remove the fcmToken
        /// </summary>
        /// <returns>username and userId (200)</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserLogout(string fcmToken)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                await _accountRepository.DeleteFCMTokenWithName(fcmToken);

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Find user containing the keywords
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public IActionResult SearchUser(string username)
        {
            try
            {
                var users = _accountRepository.ReadUserInfo(username);
                return Ok(users);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Block user
        /// </summary>
        /// <param name="blockedUserId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BlockOrUnblockUser(string blockedUserId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                var blockedUser = await _userManager.FindByIdAsync(blockedUserId);

                if (user == null || blockedUser == null)
                {
                    return BadRequest();
                }

                if (_accountRepository.IsBlockedUserExist(user.Id, blockedUser.Id) == true)
                {
                    await _accountRepository.DeleteBlockedUser(user.Id, blockedUser.Id);
                    return Ok(new { action = "unblocked" });
                }
                else
                {
                    await _accountRepository.CreateBlockedUser(user.Id, blockedUser.Id);
                    return Created(user.Id, new { action = "blocked" });
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                await _accountRepository.DeleteAllBlockedInfoWithUserId(userId);
                await _accountRepository.DeleteFCMTokenWithUserId(userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    //the account is deleted already but another device still logged in
                    return Ok();
                }
                await _userManager.DeleteAsync(user);

                var uploadImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Account", $"{userId}.jpg");
                System.IO.File.Delete(uploadImagePath);

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }


        [HttpGet]
        public IActionResult Hello()
        {
            return Ok("Hello");
        }
    }
}