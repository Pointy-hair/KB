using KnowledgeBank.Domain;
using KnowledgeBank.Persistence.Repositories;
using KnowledgeBank.Web.Helpers;
using KnowledgeBank.Web.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.IO;

namespace KnowledgeBank.Web.Controllers.API
{
    [Authorize]
    [Route("[controller]")]
    [Branch(Branch.Api)]
    public class AttachmentsController : Controller
    {
        private readonly IRepository<Attachment> _repository;
        private readonly IHostingEnvironment _hostingEnv;

        public AttachmentsController(
            IRepository<Attachment> repository,
            IOptions<AssetSettings> settings,
            IHostingEnvironment hostingEnv)
        {
            this._repository = repository;
            this._hostingEnv = hostingEnv;
        }

        [HttpGet("{id:long}")]
        public ActionResult Get(long id)
        {
            var attachment = _repository.Get(id);
            if (attachment == null)
            {
                return NotFound();
            }

            string location = attachment.Location.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar); // make path platform independent
            string fullPath = Path.Combine(_hostingEnv.ContentRootPath, location);
            FileStream stream = System.IO.File.OpenRead(fullPath);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.Name);
        }
    }
}
