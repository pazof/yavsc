using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Abstract;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Attributes.Validation;
using Yavsc.Models.Relationship;

namespace Yavsc.Server.Models.Access
{
    [Obsolete]
    public class CircleAuthorizationToFile : ICircleAuthorization
    {

        [Required]
        public long CircleId
        {
            get; set;
        }
        
        [Required]
        [YaStringLength(48)]
        [YaRegularExpression(Constants.UserFileNamePatternRegExp)]
        public string FullPath
        {
            get; set;
        }

        [ForeignKey("CircleId"), JsonIgnore]
        public virtual Circle Circle { get; set; }
    }
}
