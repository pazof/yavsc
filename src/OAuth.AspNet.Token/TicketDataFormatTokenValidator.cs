using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.DataProtection;
using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace OAuth.AspNet.Tokens
{
    public class TicketDataFormatTokenValidator : ISecurityTokenValidator
    {
        #region Constructors

        public TicketDataFormatTokenValidator(IDataProtectionProvider dataProtectionProvider, string purpose = "AccessToken") : this(dataProtectionProvider, purpose , new string [] { "v1" }) { }

        public TicketDataFormatTokenValidator(IDataProtectionProvider dataProtectionProvider, string purpose, string [] subPurposes)
        {
            if (dataProtectionProvider == null)
            {
                dataProtectionProvider = new MonoDataProtectionProvider(System.AppDomain.CurrentDomain.FriendlyName)
                .CreateProtector("profile");
            }
            _ticketDataFormat = new TicketDataFormat(dataProtectionProvider.CreateProtector(purpose, subPurposes));
        }

        #endregion

        #region non-Public Members

        private readonly TicketDataFormat _ticketDataFormat;

        private const string _serializationRegex = @"^[A-Za-z0-9-_]*$";

        private int _maximumTokenSizeInBytes = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        #endregion

        #region Public Members

        public bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        public int MaximumTokenSizeInBytes
        {
            get
            {
                return _maximumTokenSizeInBytes;
            }

            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(MaximumTokenSizeInBytes), "Negative or zero-sized tokens are invalid.");

                _maximumTokenSizeInBytes = value;
            }
        }

        public bool CanReadToken(string securityToken)
        {
            if (string.IsNullOrWhiteSpace(securityToken))
                throw new ArgumentException("Security token has no value.", nameof(securityToken));

            if (securityToken.Length * 2 > this.MaximumTokenSizeInBytes)
                return false;

            if (Regex.IsMatch(securityToken, _serializationRegex))
                return true;

            return false;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            AuthenticationTicket ticket = _ticketDataFormat.Unprotect(securityToken);

            validatedToken = null;

            return ticket?.Principal;
        }

        #endregion
    }

}
