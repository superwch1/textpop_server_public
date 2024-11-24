using MessageWebServer.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;

namespace textpop_server.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class PolicyController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PolicyController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }



        /// <summary>
        /// Get text of Private Policy
        /// </summary>
        /// <returns>Policy in text (200)</returns>
        [HttpGet]
        public IActionResult PrivacyPolicy(string? language)
        {
            try
            {
                language = language ?? "english";
                string fileContent;
                if (language.ToLower() == "chinese")
                {
                    var privacyPolicyPath = Path.Combine(_webHostEnvironment.WebRootPath, "Policy/Chinese/PrivacyPolicy.txt");
                    fileContent = System.IO.File.ReadAllText(privacyPolicyPath);
                }
                else
                {
                    var privacyPolicyPath = Path.Combine(_webHostEnvironment.WebRootPath, "Policy/English/PrivacyPolicy.txt");
                    fileContent = System.IO.File.ReadAllText(privacyPolicyPath);
                }

                return Ok(fileContent);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Get text of Terms and Conditions
        /// </summary>
        /// <returns>Policy in text (200)</returns>
        [HttpGet]
        public IActionResult TermsAndConditions(string? language)
        {
            try
            {
                language = language ?? "english";
                string fileContent;
                if (language.ToLower() == "chinese")
                {
                    var termsPath = Path.Combine(_webHostEnvironment.WebRootPath, "Policy/Chinese/TermsAndCondition.txt");
                    fileContent = System.IO.File.ReadAllText(termsPath);
                }
                else
                {
                    var termsPath = Path.Combine(_webHostEnvironment.WebRootPath, "Policy/English/TermsAndCondition.txt");
                    fileContent = System.IO.File.ReadAllText(termsPath);
                }
                return Ok(fileContent);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
