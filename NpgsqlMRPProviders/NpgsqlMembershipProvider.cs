using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

namespace Npgsql.Web
{
	public sealed class NpgsqlMembershipProvider: MembershipProvider
	{

		//
		// Global connection string, generated password length.
		//

		private int    newPasswordLength = 8;
		private string connectionString;

		//
		// Used when determining encryption key values.
		//

		private MachineKeySection machineKey;

		//
		// System.Configuration.Provider.ProviderBase.Initialize Method
		//

		public override void Initialize (string name, NameValueCollection config)
		{
			//
			// Initialize values from web.config.
			//

			if (config == null)
				throw new ArgumentNullException ("config");

			if (name == null || name.Length == 0)
				name = "NpgsqlMembershipProvider";

			if (String.IsNullOrEmpty (config ["description"])) {
				config.Remove ("description");
				config.Add ("description", "Sample Npgsql Membership provider");
			}

			// Initialize the abstract base class.
			base.Initialize (name, config);

			pApplicationName = GetConfigValue (config ["applicationName"], 
                                      System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			pMaxInvalidPasswordAttempts = Convert.ToInt32 (GetConfigValue (config ["maxInvalidPasswordAttempts"], "5"));
			pPasswordAttemptWindow = Convert.ToInt32 (GetConfigValue (config ["passwordAttemptWindow"], "10"));
			pMinRequiredNonAlphanumericCharacters = Convert.ToInt32 (GetConfigValue (config ["minRequiredNonAlphanumericCharacters"], "1"));
			pMinRequiredPasswordLength = Convert.ToInt32 (GetConfigValue (config ["minRequiredPasswordLength"], "7"));
			pPasswordStrengthRegularExpression = Convert.ToString (GetConfigValue (config ["passwordStrengthRegularExpression"], ""));
			pEnablePasswordReset = Convert.ToBoolean (GetConfigValue (config ["enablePasswordReset"], "true"));
			pEnablePasswordRetrieval = Convert.ToBoolean (GetConfigValue (config ["enablePasswordRetrieval"], "true"));
			pRequiresQuestionAndAnswer = Convert.ToBoolean (GetConfigValue (config ["requiresQuestionAndAnswer"], "false"));
			pRequiresUniqueEmail = Convert.ToBoolean (GetConfigValue (config ["requiresUniqueEmail"], "true"));
			string temp_format = config ["passwordFormat"];
			if (temp_format == null) {
				temp_format = "Hashed";
			}

			switch (temp_format) {
			case "Hashed":
				pPasswordFormat = MembershipPasswordFormat.Hashed;
				break;
			case "Encrypted":
				pPasswordFormat = MembershipPasswordFormat.Encrypted;
				break;
			case "Clear":
				pPasswordFormat = MembershipPasswordFormat.Clear;
				break;
			default:
				throw new ProviderException ("Password format not supported.");
			}

			//
			// Initialize NpgsqlConnection.
			//

			ConnectionStringSettings ConnectionStringSettings =
        ConfigurationManager.ConnectionStrings [config ["connectionStringName"]];

			if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim () == "") {
				throw new ProviderException ("Connection string cannot be blank.");
			}
			connectionString = ConnectionStringSettings.ConnectionString;

			// Get encryption and decryption key information from the configuration.
			Configuration cfg =
        WebConfigurationManager.OpenWebConfiguration (System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			machineKey = (MachineKeySection)cfg.GetSection ("system.web/machineKey");

			if (machineKey.ValidationKey.Contains ("AutoGenerate"))
			if (PasswordFormat != MembershipPasswordFormat.Clear)
				throw new ProviderException ("Hashed or Encrypted passwords " +
					"are not supported with auto-generated keys."
				);
		}


		//
		// A helper function to retrieve config values from the configuration file.
		//

		private string GetConfigValue (string configValue, string defaultValue)
		{
			if (String.IsNullOrEmpty (configValue))
				return defaultValue;

			return configValue;
		}


		//
		// System.Web.Security.MembershipProvider properties.
		//
		private string pApplicationName;
		private bool   pEnablePasswordReset;
		private bool   pEnablePasswordRetrieval;
		private bool   pRequiresQuestionAndAnswer;
		private bool   pRequiresUniqueEmail;
		private int    pMaxInvalidPasswordAttempts;
		private int    pPasswordAttemptWindow;
		private MembershipPasswordFormat pPasswordFormat;

		public override string ApplicationName {
			get { return pApplicationName; }
			set { pApplicationName = value; }
		}

		public override bool EnablePasswordReset {
			get { return pEnablePasswordReset; }
		}

		public override bool EnablePasswordRetrieval {
			get { return pEnablePasswordRetrieval; }
		}

		public override bool RequiresQuestionAndAnswer {
			get { return pRequiresQuestionAndAnswer; }
		}

		public override bool RequiresUniqueEmail {
			get { return pRequiresUniqueEmail; }
		}

		public override int MaxInvalidPasswordAttempts {
			get { return pMaxInvalidPasswordAttempts; }
		}

		public override int PasswordAttemptWindow {
			get { return pPasswordAttemptWindow; }
		}

		public override MembershipPasswordFormat PasswordFormat {
			get { return pPasswordFormat; }
		}

		private int pMinRequiredNonAlphanumericCharacters;

		public override int MinRequiredNonAlphanumericCharacters {
			get { return pMinRequiredNonAlphanumericCharacters; }
		}

		private int pMinRequiredPasswordLength;

		public override int MinRequiredPasswordLength {
			get { return pMinRequiredPasswordLength; }
		}

		private string pPasswordStrengthRegularExpression;

		public override string PasswordStrengthRegularExpression {
			get { return pPasswordStrengthRegularExpression; }
		}

		//
		// System.Web.Security.MembershipProvider methods.
		//

		//
		// MembershipProvider.ChangePassword
		//

		public override bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			if (!ValidateUser (username, oldPwd))
				return false;
			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, newPwd, true);
			OnValidatingPassword (args);
			if (args.Cancel) {
				if (args.FailureInformation != null)
					throw args.FailureInformation;
				else
					throw new MembershipPasswordException ("Change password canceled due to new password validation failure.");
			}
			int rowsAffected = 0;

			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("UPDATE Users " +
					" SET Passw = @Password, LastPasswordChangedDate = @LastPasswordChangedDate " +
					" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@Password", NpgsqlDbType.Varchar, 255).Value = EncodePassword (newPwd);
					cmd.Parameters.Add ("@LastPasswordChangedDate", DateTime.Now);
					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					rowsAffected = cmd.ExecuteNonQuery ();
					conn.Close ();
				}
			}
			if (rowsAffected > 0) {
				return true;
			}

			return false;
		}



		//
		// MembershipProvider.ChangePasswordQuestionAndAnswer
		//

		public override bool ChangePasswordQuestionAndAnswer (string username,
                  string password,
                  string newPwdQuestion,
                  string newPwdAnswer)
		{
			if (!ValidateUser (username, password))
				return false;
			int rowsAffected = 0;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("UPDATE Users " +
					" SET PasswordQuestion = @Question, PasswordAnswer = @Answer" +
					" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@Question", NpgsqlDbType.Varchar, 255).Value = newPwdQuestion;
					cmd.Parameters.Add ("@Answer", NpgsqlDbType.Varchar, 255).Value = EncodePassword (newPwdAnswer);
					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					rowsAffected = cmd.ExecuteNonQuery ();
					conn.Close ();
				}
			}
			if (rowsAffected > 0) {
				return true;
			}

			return false;
		}



		//
		// MembershipProvider.CreateUser
		//

		public override MembershipUser CreateUser (string username,
             string password,
             string email,
             string passwordQuestion,
             string passwordAnswer,
             bool isApproved,
             object providerUserKey,
             out MembershipCreateStatus status)
		{
			ValidatePasswordEventArgs args = 
        new ValidatePasswordEventArgs (username, password, true);

			OnValidatingPassword (args);
  
			if (args.Cancel) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}
			if (RequiresUniqueEmail && GetUserNameByEmail (email) != "") {
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}

			MembershipUser u = GetUser (username, false);

			if (u == null) {
				DateTime createDate = DateTime.Now;

				if (providerUserKey == null) {
					providerUserKey = Guid.NewGuid ();
				} else {
					if (!(providerUserKey is Guid)) {
						status = MembershipCreateStatus.InvalidProviderUserKey;
						return null;
					}
				}

				using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
					using (NpgsqlCommand cmd = new NpgsqlCommand ("INSERT INTO Users " +
					" (PKID, Username, Passw, Email, PasswordQuestion, " +
					" PasswordAnswer, IsApproved," +
					" Comment, CreationDate, LastPasswordChangedDate, LastActivityDate," +
					" ApplicationName, IsLockedOut, LastLockedOutDate," +
					" FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, " +
					" FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)" +
					" Values(@PKID, @Username, @Password, @Email, @PasswordQuestion, @PasswordAnswer, @IsApproved," +
					"@Comment, @CreationDate, @LastPasswordChangedDate, @LastActivityDate, " +
					"@ApplicationName,@IsLockedOut, @LastLockedOutDate," +
					"@FailedPasswordAttemptCount , @FailedPasswordAttemptWindowStart, " + 
					" @FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart)", conn)) {

						cmd.Parameters.Add ("@PKID", NpgsqlDbType.Varchar).Value = providerUserKey;
						cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
						cmd.Parameters.Add ("@Password", NpgsqlDbType.Varchar, 255).Value = EncodePassword (password);
						cmd.Parameters.Add ("@Email", NpgsqlDbType.Varchar, 128).Value = email;
						cmd.Parameters.Add ("@PasswordQuestion", NpgsqlDbType.Varchar, 255).Value = passwordQuestion;
						cmd.Parameters.Add ("@PasswordAnswer", NpgsqlDbType.Varchar, 255).Value = EncodePassword (passwordAnswer);
						cmd.Parameters.Add ("@IsApproved", NpgsqlDbType.Bit).Value = isApproved;
						cmd.Parameters.Add ("@Comment", NpgsqlDbType.Varchar, 255).Value = "";
						cmd.Parameters.Add ("@CreationDate", createDate);
						cmd.Parameters.Add ("@LastPasswordChangedDate", createDate);
						cmd.Parameters.Add ("@LastActivityDate", createDate);
						cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
						cmd.Parameters.Add ("@IsLockedOut", NpgsqlDbType.Bit).Value = false;
						cmd.Parameters.Add ("@LastLockedOutDate", createDate);
						cmd.Parameters.Add ("@FailedPasswordAttemptCount", NpgsqlDbType.Integer).Value = 0;
						cmd.Parameters.Add ("@FailedPasswordAttemptWindowStart", createDate);
						cmd.Parameters.Add ("@FailedPasswordAnswerAttemptCount", NpgsqlDbType.Integer).Value = 0;
						cmd.Parameters.Add ("@FailedPasswordAnswerAttemptWindowStart", createDate);
						conn.Open ();
						int recAdded = cmd.ExecuteNonQuery ();
						if (recAdded > 0) {
							status = MembershipCreateStatus.Success;
						} else {
							status = MembershipCreateStatus.UserRejected;
						}
						conn.Close ();
					}
				}
				return GetUser (username, false);      
			} else {
				status = MembershipCreateStatus.DuplicateUserName;
			}
			return null;
		}

		//
		// MembershipProvider.DeleteUser
		//
		/// <Docs>
		/// To be added.
		/// </Docs>
		/// <summary>
		/// Delete the user from given name.
		/// </summary>
		/// <remarks>
		/// The <c>deleteAllRelatedData</c> parameter usage has to be implemented.
		/// </remarks>
		/// <returns>
		/// The user.
		/// </returns>
		/// <param name='username'>
		/// If set to <c>true</c> username.
		/// </param>
		/// <param name='deleteAllRelatedData'>
		/// If set to <c>true</c> delete all related data.
		/// </param>
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			int rowsAffected = 0;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("DELETE FROM Users " +
					" WHERE Username = @Username AND Applicationname = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					rowsAffected = cmd.ExecuteNonQuery ();
					if (deleteAllRelatedData) {
						// TODO Process commands to delete all data for the user in the database.
					}
					conn.Close ();
				}
			}
			return (rowsAffected > 0);
		}

		//
		// MembershipProvider.GetAllUsers
		//

		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			MembershipUserCollection users = new MembershipUserCollection ();
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT Count(*) FROM Users " +
				"WHERE ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = ApplicationName;
					conn.Open ();
					totalRecords = 0;
					totalRecords = (int)((long)cmd.ExecuteScalar ());

					if (totalRecords > 0) {
						cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
							" Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
							" LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
							" FROM Users " + 
							" WHERE ApplicationName = @ApplicationName " +
							" ORDER BY Username Asc";
						using (NpgsqlDataReader reader = cmd.ExecuteReader ()) {

							int counter = 0;
							int startIndex = pageSize * pageIndex;
							int endIndex = startIndex + pageSize - 1;

							while (reader.Read()) {
								if (counter >= startIndex) {
									MembershipUser u = GetUserFromReader (reader);
									users.Add (u);
								}

								if (counter >= endIndex) {
									cmd.Cancel ();
								}

								counter++;
							}
							reader.Close ();

						}
					}
					conn.Close ();
				}
			}
			return users;
		}


		//
		// MembershipProvider.GetNumberOfUsersOnline
		//

		public override int GetNumberOfUsersOnline ()
		{
			int numOnline = 0;
			TimeSpan onlineSpan = new TimeSpan (0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
			DateTime compareTime = DateTime.Now.Subtract (onlineSpan);

			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT Count(*) FROM Users " +
					" WHERE LastActivityDate > @CompareDate AND ApplicationName = @ApplicationName", conn)) {

					cmd.Parameters.Add ("@CompareDate", compareTime);
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

				

					conn.Open ();

					numOnline = (int)cmd.ExecuteScalar ();
				}
				conn.Close ();
			}

			return numOnline;
		}



		//
		// MembershipProvider.GetPassword
		//

		public override string GetPassword (string username, string answer)
		{
			string password = "";
			string passwordAnswer = "";

			if (!EnablePasswordRetrieval) {
				throw new ProviderException ("Password Retrieval Not Enabled.");
			}

			if (PasswordFormat == MembershipPasswordFormat.Hashed) {
				throw new ProviderException ("Cannot retrieve Hashed passwords.");
			}

			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT Passw, PasswordAnswer, IsLockedOut FROM Users " +
					" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {

					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;


					NpgsqlDataReader reader = null;


					conn.Open ();

					using (reader = cmd.ExecuteReader ()) {

						if (reader.HasRows) {
							reader.Read ();

							if (reader.GetBoolean (2))
								throw new MembershipPasswordException ("The supplied user is locked out.");

							password = reader.GetString (0);
							passwordAnswer = reader.GetString (1);
						} else {
							throw new MembershipPasswordException ("The supplied user name is not found.");
						}

						reader.Close ();				
					}
				}
				conn.Close ();
			}

			if (RequiresQuestionAndAnswer && !CheckPassword (answer, passwordAnswer)) {
				UpdateFailureCount (username, "passwordAnswer");
				throw new MembershipPasswordException ("Incorrect password answer.");
			}
			if (PasswordFormat == MembershipPasswordFormat.Encrypted) {
				password = UnEncodePassword (password);
			}
			return password;
		}

		//
		// MembershipProvider.GetUser(string, bool)
		//
		/// <summary>
		/// Gets the user as a MembershipUser object
		/// </summary>
		/// <returns>
		/// The user.
		/// </returns>
		/// <param name='username'>
		/// The user name to search.
		/// </param>
		/// <param name='userIsOnline'>
		/// Only return the user when it's online.
		/// </param>
		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			MembershipUser u = null;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT PKID, Username, Email, PasswordQuestion," +
					" Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
					" LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
					" FROM Users WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {

					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					using (NpgsqlDataReader reader =  cmd.ExecuteReader ()) {
						if (reader.HasRows) {
							reader.Read ();
							u = GetUserFromReader (reader);
          
							if (userIsOnline) {
								NpgsqlCommand updateCmd = new NpgsqlCommand ("UPDATE Users " +
									"SET LastActivityDate = @LastActivityDate " +
									"WHERE Username = @Username AND Applicationname = @ApplicationName", conn);

								updateCmd.Parameters.Add ("@LastActivityDate",  DateTime.Now);
								updateCmd.Parameters.Add ("@Username", username);
								updateCmd.Parameters.Add ("@ApplicationName", pApplicationName);

								updateCmd.ExecuteNonQuery ();
							}
						}
						reader.Close ();
					}
				}
				conn.Close ();
			}
			return u;      
		}


		//
		// MembershipProvider.GetUser(object, bool)
		//
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			MembershipUser u = null;
			
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT PKID, Username, Email, PasswordQuestion," +
				" Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
				" LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
				" FROM Users WHERE PKID = @PKID", conn)) {

					cmd.Parameters.Add ("@PKID", NpgsqlDbType.Varchar).Value = providerUserKey;
					conn.Open ();

					using (NpgsqlDataReader reader = cmd.ExecuteReader ()) {
						if (reader.HasRows) {
							reader.Read ();
							u = GetUserFromReader (reader);
          
							if (userIsOnline) {
								NpgsqlCommand updateCmd = new NpgsqlCommand ("UPDATE Users " +
									"SET LastActivityDate = @LastActivityDate " +
									"WHERE PKID = @PKID", conn);
								updateCmd.Parameters.Add ("@LastActivityDate", DateTime.Now);
								updateCmd.Parameters.Add ("@PKID", providerUserKey);
								updateCmd.ExecuteNonQuery ();
							}
						}
						reader.Close ();
					}
				}
				conn.Close ();
			}

			return u;      
		}


		//
		// GetUserFromReader
		//    A helper function that takes the current row from the NpgsqlDataReader
		// and hydrates a MembershiUser from the values. Called by the 
		// MembershipUser.GetUser implementation.
		//

		private MembershipUser GetUserFromReader (NpgsqlDataReader reader)
		{
			object providerUserKey = reader.GetValue (0);
			string username = reader.GetString (1);
			string email = reader.GetString (2);

			string passwordQuestion = "";
			if (reader.GetValue (3) != DBNull.Value)
				passwordQuestion = reader.GetString (3);

			string comment = "";
			if (reader.GetValue (4) != DBNull.Value)
				comment = reader.GetString (4);

			bool isApproved = reader.GetBoolean (5);
			bool isLockedOut = reader.GetBoolean (6);
			DateTime creationDate = reader.GetDateTime (7);

			DateTime lastLoginDate = new DateTime ();
			if (reader.GetValue (8) != DBNull.Value)
				lastLoginDate = reader.GetDateTime (8);

			DateTime lastActivityDate = reader.GetDateTime (9);
			DateTime lastPasswordChangedDate = reader.GetDateTime (10);

			DateTime lastLockedOutDate = new DateTime ();
			if (reader.GetValue (11) != DBNull.Value)
				lastLockedOutDate = reader.GetDateTime (11);
      
			MembershipUser u = new MembershipUser (this.Name,
                                            username,
                                            providerUserKey,
                                            email,
                                            passwordQuestion,
                                            comment,
                                            isApproved,
                                            isLockedOut,
                                            creationDate,
                                            lastLoginDate,
                                            lastActivityDate,
                                            lastPasswordChangedDate,
                                            lastLockedOutDate);

			return u;
		}


		//
		// MembershipProvider.UnlockUser
		//

		public override bool UnlockUser (string username)
		{
			int rowsAffected = 0;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("UPDATE Users " +
					" SET IsLockedOut = False, LastLockedOutDate = @LastLockedOutDate " +
					" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@LastLockedOutDate", DateTime.Now);
					cmd.Parameters.Add ("@Username", username);
					cmd.Parameters.Add ("@ApplicationName", pApplicationName);
					conn.Open ();
					rowsAffected = cmd.ExecuteNonQuery ();
					conn.Close ();
				}
			}
			if (rowsAffected > 0)
				return true;

			return false;      
		}


		//
		// MembershipProvider.GetUserNameByEmail
		//

		public override string GetUserNameByEmail (string email)
		{
			string username = "";
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT Username" +
				" FROM Users WHERE Email = @Email AND ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@Email", NpgsqlDbType.Varchar, 128).Value = email;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					username = (string)cmd.ExecuteScalar ();
					conn.Close ();
				}
			}
			if (username == null)
				username = "";
			return username;
		}

		//
		// MembershipProvider.ResetPassword
		//

		public override string ResetPassword (string username, string answer)
		{
			int rowsAffected = 0;
			if (!EnablePasswordReset) {
				throw new NotSupportedException ("Password reset is not enabled.");
			}

			if (answer == null && RequiresQuestionAndAnswer) {
				UpdateFailureCount (username, "passwordAnswer");
				throw new ProviderException ("Password answer required for password reset.");
			}

			string newPassword = 
        System.Web.Security.Membership.GeneratePassword (newPasswordLength, MinRequiredNonAlphanumericCharacters);

			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, newPassword, true);

			OnValidatingPassword (args);
  
			if (args.Cancel)
			if (args.FailureInformation != null)
				throw args.FailureInformation;
			else
				throw new MembershipPasswordException ("Reset password canceled due to password validation failure.");

			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT PasswordAnswer, IsLockedOut FROM Users " +
				" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {

					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;


					string passwordAnswer = "";
					conn.Open ();
			
					using (NpgsqlDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleRow)) {

						if (reader.HasRows) {
							reader.Read ();

							if (reader.GetBoolean (1))
								throw new MembershipPasswordException ("The supplied user is locked out.");

							passwordAnswer = reader.GetString (0);
						} else {
							throw new MembershipPasswordException ("The supplied user name is not found.");
						}

						if (RequiresQuestionAndAnswer && !CheckPassword (answer, passwordAnswer)) {
							UpdateFailureCount (username, "passwordAnswer");

							throw new MembershipPasswordException ("Incorrect password answer.");
						}

						NpgsqlCommand updateCmd = new NpgsqlCommand ("UPDATE Users " +
							" SET Passw = @Password, LastPasswordChangedDate = @LastPasswordChangedDate" +
							" WHERE Username = @Username AND ApplicationName = @ApplicationName AND IsLockedOut = False", conn);

						updateCmd.Parameters.Add ("@Password", NpgsqlDbType.Varchar, 255).Value = EncodePassword (newPassword);
						updateCmd.Parameters.Add ("@LastPasswordChangedDate", DateTime.Now);
						updateCmd.Parameters.Add ("@Username", username);
						updateCmd.Parameters.Add ("@ApplicationName", pApplicationName);

						rowsAffected = updateCmd.ExecuteNonQuery ();
			
						reader.Close ();
					}
					conn.Close ();
				}
			}

			if (rowsAffected > 0) {
				return newPassword;
			} else {
				throw new MembershipPasswordException ("User not found, or user is locked out. Password not Reset.");
			}
		}


		//
		// MembershipProvider.UpdateUser
		//

		public override void UpdateUser (MembershipUser user)
		{
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("UPDATE Users " +
				" SET Email = @Email, Comment = @Comment," +
				" IsApproved = @IsApproved" +
				" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {
					cmd.Parameters.Add ("@Email", NpgsqlDbType.Varchar, 128).Value = user.Email;
					cmd.Parameters.Add ("@Comment", NpgsqlDbType.Varchar, 255).Value = user.Comment;
					cmd.Parameters.Add ("@IsApproved", NpgsqlDbType.Bit).Value = user.IsApproved;
					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = user.UserName;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
					conn.Open ();
					cmd.ExecuteNonQuery ();
					conn.Close ();
				}
			}
		}


		//
		// MembershipProvider.ValidateUser
		//

		public override bool ValidateUser (string username, string password)
		{
			bool isValid = false;

			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT Passw, IsApproved FROM Users " +
				" WHERE Username = @Username AND ApplicationName = @ApplicationName AND IsLockedOut = False", conn)) {

					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

		
					bool isApproved = false;
					string pwd = "";

					conn.Open ();
					bool userfound = false;
					using (NpgsqlDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleRow)) {
						userfound = reader.HasRows;
						if (userfound) {
							reader.Read ();
							pwd = reader.GetString (0);
							isApproved = reader.GetBoolean (1);
						} 

						reader.Close ();
					}
					if (userfound) {
						if (CheckPassword (password, pwd)) {
							if (isApproved) {
								isValid = true;

								NpgsqlCommand updateCmd = new NpgsqlCommand ("UPDATE Users SET LastLoginDate = @LastLoginDate" +
									" WHERE Username = @Username AND ApplicationName = @ApplicationName", conn);

								updateCmd.Parameters.Add ("@LastLoginDate", DateTime.Now);
								updateCmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
								updateCmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
 
								updateCmd.ExecuteNonQuery ();
							}
						} else {
							UpdateFailureCount (username, "password");
						}
					}
					conn.Close ();
				}
			}

			return isValid;
		}



		/// <summary>
		/// A helper method that performs the checks and updates associated with
		/// password failure tracking.
		/// </summary>
		/// <param name='username'>
		/// User name.
		/// </param>
		/// <param name='failureType'>
		/// Failure type, one of <c>password</c>, <c>passwordAnswer</c>
		/// </param>
		private void UpdateFailureCount (string username, string failureType)
		{
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT FailedPasswordAttemptCount, " +
				"  FailedPasswordAttemptWindowStart, " +
				"  FailedPasswordAnswerAttemptCount, " +
				"  FailedPasswordAnswerAttemptWindowStart " + 
				"  FROM Users " +
				"  WHERE Username = @Username AND ApplicationName = @ApplicationName", conn)) {

					cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

			
					DateTime windowStart = new DateTime ();
					int failureCount = 0;

			
					conn.Open ();
					using (NpgsqlDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleRow)) {

						if (reader.HasRows) {
							reader.Read ();

							if (failureType == "password") {
								failureCount = reader.GetInt32 (0);
								windowStart = reader.GetDateTime (1);
							}

							if (failureType == "passwordAnswer") {
								failureCount = reader.GetInt32 (2);
								windowStart = reader.GetDateTime (3);
							}
						}

						reader.Close ();
					}
					DateTime windowEnd = windowStart.AddMinutes (PasswordAttemptWindow);

					if (failureCount == 0 || DateTime.Now > windowEnd) {
						// First password failure or outside of PasswordAttemptWindow. 
						// Start a new password failure count from 1 and a new window starting now.

						if (failureType == "password")
							cmd.CommandText = "UPDATE Users " +
								"  SET FailedPasswordAttemptCount = @Count, " +
								"      FailedPasswordAttemptWindowStart = @WindowStart " +
								"  WHERE Username = @Username AND ApplicationName = @ApplicationName";

						if (failureType == "passwordAnswer")
							cmd.CommandText = "UPDATE Users " +
								"  SET FailedPasswordAnswerAttemptCount = @Count, " +
								"      FailedPasswordAnswerAttemptWindowStart = @WindowStart " +
								"  WHERE Username = @Username AND ApplicationName = @ApplicationName";

						cmd.Parameters.Clear ();

						cmd.Parameters.Add ("@Count", NpgsqlDbType.Integer).Value = 1;
						cmd.Parameters.Add ("@WindowStart", DateTime.Now);
						cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
						cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

						cmd.ExecuteNonQuery ();

					} else {
						if (failureCount++ >= MaxInvalidPasswordAttempts) {
							// Password attempts have exceeded the failure threshold. Lock out
							// the user.

							cmd.CommandText = "UPDATE Users " +
								"  SET IsLockedOut = @IsLockedOut, LastLockedOutDate = @LastLockedOutDate " +
								"  WHERE Username = @Username AND ApplicationName = @ApplicationName";

							cmd.Parameters.Clear ();

							cmd.Parameters.Add ("@IsLockedOut", NpgsqlDbType.Bit).Value = true;
							cmd.Parameters.Add ("@LastLockedOutDate", DateTime.Now);
							cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
							cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

							cmd.ExecuteNonQuery ();

						} else {
							// Password attempts have not exceeded the failure threshold. Update
							// the failure counts. Leave the window the same.

							if (failureType == "password")
								cmd.CommandText = "UPDATE Users " +
									"  SET FailedPasswordAttemptCount = @Count" +
									"  WHERE Username = @Username AND ApplicationName = @ApplicationName";

							if (failureType == "passwordAnswer")
								cmd.CommandText = "UPDATE Users " +
									"  SET FailedPasswordAnswerAttemptCount = @Count" +
									"  WHERE Username = @Username AND ApplicationName = @ApplicationName";

							cmd.Parameters.Clear ();

							cmd.Parameters.Add ("@Count", NpgsqlDbType.Integer).Value = failureCount;
							cmd.Parameters.Add ("@Username", NpgsqlDbType.Varchar, 255).Value = username;
							cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;

							cmd.ExecuteNonQuery ();
						}
					}
				}
				conn.Close ();
			} 
		}


		//
		// CheckPassword
		//   Compares password values based on the MembershipPasswordFormat.
		//

		private bool CheckPassword (string password, string dbpassword)
		{
			string pass1 = password;
			string pass2 = dbpassword;

			switch (PasswordFormat) {
			case MembershipPasswordFormat.Encrypted:
				pass2 = UnEncodePassword (dbpassword);
				break;
			case MembershipPasswordFormat.Hashed:
				pass1 = EncodePassword (password);
				break;
			default:
				break;
			}
			if (pass1 == pass2) {
				return true;
			}
			return false;
		}


		//
		// EncodePassword
		//   Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
		//

		private string EncodePassword (string password)
		{
			string encodedPassword = password;

			switch (PasswordFormat) {
			case MembershipPasswordFormat.Clear:
				break;
			case MembershipPasswordFormat.Encrypted:
				encodedPassword = 
            Convert.ToBase64String (EncryptPassword (Encoding.Unicode.GetBytes (password)));
				break;
			case MembershipPasswordFormat.Hashed:
				HMACSHA1 hash = new HMACSHA1 ();
				hash.Key = HexToByte (machineKey.ValidationKey);
				encodedPassword = 
            Convert.ToBase64String (hash.ComputeHash (Encoding.Unicode.GetBytes (password)));
				break;
			default:
				throw new ProviderException ("Unsupported password format.");
			}

			return encodedPassword;
		}


		//
		// UnEncodePassword
		//   Decrypts or leaves the password clear based on the PasswordFormat.
		//

		private string UnEncodePassword (string encodedPassword)
		{
			string password = encodedPassword;

			switch (PasswordFormat) {
			case MembershipPasswordFormat.Clear:
				break;
			case MembershipPasswordFormat.Encrypted:
				password = 
            Encoding.Unicode.GetString (DecryptPassword (Convert.FromBase64String (password)));
				break;
			case MembershipPasswordFormat.Hashed:
				throw new ProviderException ("Cannot unencode a hashed password.");
			default:
				throw new ProviderException ("Unsupported password format.");
			}

			return password;
		}

		//
		// HexToByte
		//   Converts a hexadecimal string to a byte array. Used to convert encryption
		// key values from the configuration.
		//

		private byte[] HexToByte (string hexString)
		{
			byte[] returnBytes = new byte[hexString.Length / 2];
			for (int i = 0; i < returnBytes.Length; i++)
				returnBytes [i] = Convert.ToByte (hexString.Substring (i * 2, 2), 16);
			return returnBytes;
		}


		//
		// MembershipProvider.FindUsersByName
		//

		public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			MembershipUserCollection users = new MembershipUserCollection ();
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT count(*)" +
						" FROM Users " + 
						" WHERE Username LIKE @UsernameSearch AND ApplicationName = @ApplicationName ", conn)) {
					totalRecords = (int)cmd.ExecuteScalar ();
				}
				if (totalRecords > 0)
					using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT PKID, Username, Email, PasswordQuestion," +
						" Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
						" LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " +
						" FROM Users " + 
						" WHERE Username LIKE @UsernameSearch AND ApplicationName = @ApplicationName " +
						" ORDER BY Username Asc", conn)) {
						cmd.Parameters.Add ("@UsernameSearch", NpgsqlDbType.Varchar, 255).Value = usernameToMatch;
						cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = pApplicationName;
						conn.Open ();
						using (NpgsqlDataReader reader = cmd.ExecuteReader ()) {
							int counter = 0;
							int startIndex = pageSize * pageIndex;
							int endIndex = startIndex + pageSize - 1;

							while (reader.Read()) {
								if (counter >= startIndex) {
									MembershipUser u = GetUserFromReader (reader);
									users.Add (u);
								}
								if (counter >= endIndex) {
									cmd.Cancel ();
								}
								counter++;
							}
							reader.Close ();
						}
						conn.Close ();
					}
			}
			return users;
		}

		//
		// MembershipProvider.FindUsersByEmail
		//

		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			MembershipUserCollection users = new MembershipUserCollection ();
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT count(*) " +
					" FROM Users " + 
					" WHERE Email LIKE @EmailSearch AND ApplicationName = @ApplicationName ", conn)) {
					totalRecords = (int)cmd.ExecuteScalar ();
				}

				using (NpgsqlCommand cmd = new NpgsqlCommand ("SELECT PKID, Username, Email, PasswordQuestion," +
					" Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
					" LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
					" FROM Users " + 
					" WHERE Email LIKE @EmailSearch AND ApplicationName = @ApplicationName " +
					" ORDER BY Username Asc", conn)) {
					cmd.Parameters.Add ("@EmailSearch", NpgsqlDbType.Varchar, 255).Value = emailToMatch;
					cmd.Parameters.Add ("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = ApplicationName;
					conn.Open ();
					using (NpgsqlDataReader reader = cmd.ExecuteReader ()) {
						int counter = 0;
						int startIndex = pageSize * pageIndex;
						int endIndex = startIndex + pageSize - 1;

						while (reader.Read()) {
							if (counter >= startIndex) {
								MembershipUser u = GetUserFromReader (reader);
								users.Add (u);
							}
							if (counter >= endIndex) {
								cmd.Cancel ();
							}
							counter++;
						}
						reader.Close ();
					}
					conn.Close ();
				}
			}
			return users;
		}

	}
}
