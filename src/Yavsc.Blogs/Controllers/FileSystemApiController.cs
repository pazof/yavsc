
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Controllers
{
    using Yavsc.Attributes.Validation;
    using Yavsc.Models;
    using Yavsc.Models.Blog;
    using Yavsc.Models.Billing;
    using Yavsc.Exceptions;
    using Yavsc.Server.Helpers;
    using Yavsc.Abstract.Helpers;
    using Yavsc.Server.Models.FileSystem;
    using Yavsc.ViewModels.UserFiles;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    [Authorize,Route(APIPrefix + "/fs")]
    public partial class FileSystemApiController : Controller
    {
        readonly ApplicationDbContext dbContext;
        private readonly IAuthorizationService AuthorizationService;
        private readonly ILogger _logger;
        private readonly SiteSettings siteSettings;

        public FileSystemApiController(ApplicationDbContext context,
        IAuthorizationService authorizationService,
        ILoggerFactory loggerFactory,
        IOptions<SiteSettings> siteSettings)

        {
            AuthorizationService = authorizationService;
            dbContext = context;
            _logger = loggerFactory.CreateLogger<FileSystemApiController>();
            this.siteSettings = siteSettings.Value;
        }

        // GET: api/v1/fs
        [HttpGet()]
        public IActionResult Get()
        {
            return GetDir(null);
        }

        // GET: api/v1/fs/{subdir}
        // Liste les fichiers du sous-répertoire personnel de l'utilisateur,
        // en enrichissant chaque entrée de l'identifiant de métadonnées
        // UploadedFile.Id quand une ligne existe (cf. POST ci-dessous).
        [HttpGet("{*subdir}")]
        public IActionResult GetDir([ValidRemoteUserFilePath] string subdir="")
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var uid = User.GetUserId();
            // The personal-storage tree is keyed by the user's login
            // (UserName): EnsureDestinationDirectory writes under
            // {Blog}/{UserName}, the download endpoint and avatar paths
            // use user.UserName too. GetUserId() returns the "sub" claim
            // (a GUID in production), so passing uid here made the listing
            // look under {Blog}/{sub-GUID} — an empty tree, hence the
            // "Mes fichiers" page listing nothing. Resolve the login and
            // list under it. OwnerId on UploadedFile stays "sub" (uid),
            // so EnrichWithFileIds still queries by uid.
            var user = dbContext.Users.SingleOrDefault(u => u.Id == uid);
            if (user == null) return NotFound();
            var files = FileSystemHelpers.GetUserFiles(siteSettings, user.UserName, subdir);
            EnrichWithFileIds(files, uid, subdir);
            return Ok(files);
        }

        // Reporte l'UploadedFile.Id (quand il existe) sur chaque entrée
        // retournée par la liste, pour que le client lourd puisse attacher
        // le fichier à un devis/demande par référence.
        void EnrichWithFileIds(UserDirectoryInfo dir, string ownerId, string subdir)
        {
            if (dir?.Files == null || dir.Files.Length == 0) return;
            var relPaths = dir.Files
                .Select(f => RelPath(subdir, f.Name))
                .ToHashSet();
            var byPath = dbContext.UploadedFiles
                .Where(u => u.OwnerId == ownerId && relPaths.Contains(u.Path))
                .ToDictionary(u => u.Path, u => u.Id);
            foreach (var f in dir.Files)
            {
                var rel = RelPath(subdir, f.Name);
                if (byPath.TryGetValue(rel, out var id)) f.Id = id;
            }
        }

        // Chemin relatif au racine de l'espace perso de l'utilisateur,
        // tel que stocké dans UploadedFile.Path (sans segment utilisateur).
        static string RelPath(string subdir, string fileName)
        {
            // Normalize with '/' for cross-platform consistency in DB.
            if (string.IsNullOrWhiteSpace(subdir)) return fileName;
            return $"{subdir.TrimStart('/')}/{fileName}";
        }

        [HttpPost("{*subdir}")]
        public IActionResult Post([ValidRemoteUserFilePath] string subdir="")
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            string destDir = null;
            List<FileReceivedInfo> received = new List<FileReceivedInfo>();
            InvalidPathException pathex = null;
            try {
                destDir = User.EnsureDestinationDirectory(subdir, siteSettings);
            } catch (InvalidPathException ex) {
                pathex = ex;
            }
            if (pathex!=null)
            {
                _logger.LogError($"invalid sub path: '{subdir}'.");
                return BadRequest(pathex);
            }
            _logger.LogInformation($"Receiving files, saved in '{destDir}' (specified as '{subdir}').");

            var uid = User.GetUserId();
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            int i=0;
            _logger.LogInformation($"Receiving {Request.Form.Files.Count} files.");

            foreach (var f in Request.Form.Files)
            {
                var item = user.ReceiveUserFile(destDir, f);
                if (!item.QuotaOffense)
                {
                    // Persiste une ligne de métadonnées UploadedFile,
                    // propriétaire = l'utilisateur courant, chemin relatif
                    // à son espace perso. Upsert sur (OwnerId, Path) pour
                    // le re-téléversement (même fichier écrasé sur disque).
                    var rel = RelPath(subdir, item.FileName);
                    var existing = dbContext.UploadedFiles
                        .FirstOrDefault(u => u.OwnerId == uid && u.Path == rel);
                    if (existing == null)
                    {
                        var uploaded = new UploadedFile
                        {
                            OwnerId = uid,
                            Path = rel,
                            ContentType = f.ContentType,
                            Length = f.Length
                        };
                        dbContext.UploadedFiles.Add(uploaded);
                        dbContext.SaveChanges(User.GetUserId());
                        item.FileId = uploaded.Id;
                    }
                    else
                    {
                        existing.ContentType = f.ContentType;
                        existing.Length = f.Length;
                        dbContext.SaveChanges(User.GetUserId());
                        item.FileId = existing.Id;
                    }
                    item.Length = f.Length;
                    item.ContentType = f.ContentType;
                }
                received.Add(item);
                _logger.LogInformation($"Received  '{item.FileName}'.");
                if (item.QuotaOffense)
                    break;
                i++;
            };
            return Ok(received);
        }

        // GET: api/v1/fs/file/{fileId}
        // Télécharge une pièce jointe référencée par son UploadedFile.Id.
        // Autorise : le propriétaire du fichier, ou toute partie (client /
        // fournisseur) d'un devis ou d'une demande auquel ce fichier est
        // rattaché via EstimateAttachedFile / QueryAttachedFile. Les octets
        // sont lus depuis l'espace perso du propriétaire, sur le disque de
        // ce host Blogs (le host API ne possède pas ces octets).
        [HttpGet("file/{fileId:long}")]
        public IActionResult GetFile(long fileId)
        {
            var file = dbContext.UploadedFiles.FirstOrDefault(u => u.Id == fileId);
            if (file == null) return NotFound(new { error = "file not found" });

            var uid = User.GetUserId();
            if (!CanReadFile(file, uid))
                return Forbid();

            var ownerUserName = dbContext.Users
                .Where(u => u.Id == file.OwnerId)
                .Select(u => u.UserName)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(ownerUserName))
                return NotFound(new { error = "owner not found" });

            var physicalPath = Path.Combine(siteSettings.Blog, ownerUserName, file.Path ?? "");
            var fi = new FileInfo(physicalPath);
            if (!fi.Exists) return NotFound(new { error = "file not on disk" });

            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream" : file.ContentType;
            var stream = fi.OpenRead();
            return File(stream, contentType, fi.Name);
        }

        // Le propriétaire peut toujours lire son fichier ; sinon, il faut
        // que le fichier soit rattaché à un devis ou une demande dont
        // l'appelant est partie (client ou fournisseur). Les admin sont
        // autorisés comme pour le reste du workflow.
        bool CanReadFile(UploadedFile file, string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            if (file.OwnerId == uid) return true;

            if (dbContext.EstimateAttachedFiles.Any(a =>
                a.FileId == file.Id &&
                (a.Estimate.ClientId == uid || a.Estimate.OwnerId == uid)))
                return true;

            if (dbContext.QueryAttachedFiles.Any(a =>
                a.FileId == file.Id &&
                (a.Command.ClientId == uid || a.Command.PerformerId == uid)))
                return true;

            return false;
        }

        [Route("/api/fsc/addquota/{uname}/{len}")]
        [Authorize("AdministratorOnly")]
        public IActionResult AddQuota(string uname, int len)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.FirstOrDefault(
                u => u.UserName == uname
            );
            if (user==null) return new BadRequestObjectResult(new { error = "no such use" });
            user.AddQuota(len);
            dbContext.SaveChanges(uid);
            return Ok(len);
        }

        [HttpPost]
        [Route("/api/fsc/mvftd")]
        [Authorize()]
        public IActionResult MoveFile([FromBody] RenameFileQuery query)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            var info = user.MoveUserFileToDir(query.Id, query.To, siteSettings);
            if (!info.Done) return new BadRequestObjectResult(info);
            return Ok(new { moved = query.Id });
        }

        [HttpPost]
        [Route("/api/fsc/mvf")]
        [Authorize()]
        public IActionResult RenameFile([FromBody] RenameFileQuery query)
        {
            if (!ModelState.IsValid) {
                var idvr = new ValidRemoteUserFilePathAttribute();

                return this.BadRequest(new { id = idvr.IsValid(query.Id), to = idvr.IsValid(query.To), errors = ModelState });
            }
            _logger.LogInformation($"Valid move query: {query.Id} => {query.To}");
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Users.Single(
                u => u.Id == uid
            );
            try {
                if (Config.UserFilesOptions.FileProvider.GetFileInfo(Path.Combine(user.UserName, query.Id)).Exists)
                {
                    var result = user.MoveUserFile(query.Id, query.To, siteSettings);
                    if (!result.Done) return new BadRequestObjectResult(result);
                }
                else {
                    var result = user.MoveUserDir(query.Id, query.To, siteSettings);
                    if (!result.Done) return new BadRequestObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(
                    new FsOperationInfo {
                        Done = false,
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message
                });
            }
            return Ok();
        }

        [HttpDelete("{*id}")]
        public IActionResult RemoveDirOrFile ([ValidRemoteUserFilePath] string id)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);

            var user = dbContext.Users.Single(
                u => u.Id == User.GetUserId()
            );

            try {
                var result = user.DeleteUserDirOrFile(id, siteSettings);
                if (!result.Done)
                    return new BadRequestObjectResult(result);
            }

            catch (Exception ex)
            {
                return new BadRequestObjectResult(
                    new FsOperationInfo {
                        Done = false,
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message
                });
            }
            return Ok(new { deleted=id });
        }


    }

}
