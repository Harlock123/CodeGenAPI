using System;
using System.Security.Cryptography;
using System.Text;

namespace CodeGenAPI
{
    public class EncryptDecrypt
    {
        public EncryptDecrypt()
        {
			
		}
		
		public string Encrypt(string raw)
		{

			using (var csp = new AesCryptoServiceProvider())
			{
				string encrypted = null;

				try
				{
					ICryptoTransform e = GetCryptoTransform(csp, true);
					byte[] inputBuffer = Encoding.UTF8.GetBytes(raw);
					byte[] output = e.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
					encrypted = Convert.ToBase64String(output);

				}
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
				catch (Exception ex)
				{
					// We should do somthing here
				}
#pragma warning restore CS0168 // The variable 'ex' is declared but never used

				return encrypted;
			}

		}

		public string Decrypt(string encrypted)
		{


			//// This is kindof dumb but if it flags as sanatized?
			//encrypted = System.Security.SecurityElement.Escape(encrypted);

			//encrypted = encrypted.Replace("&quot", '"'.ToString()).Replace("&lt", "<").Replace("&gt", ">").Replace("&apos", "'").Replace("&amp", "&");
			//// end of dumbness

			//encrypted = encrypted.Replace("!nf", "/").Replace("!ng", "+");
			using (var csp = new AesCryptoServiceProvider())
			{
				string decypted = null;

				try
				{
					var d = GetCryptoTransform(csp, false);
					byte[] output = Convert.FromBase64String(encrypted);
					byte[] decryptedOutput = d.TransformFinalBlock(output, 0, output.Length);
					decypted = Encoding.UTF8.GetString(decryptedOutput);

				}
				
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
			catch (Exception ex)
			{
				// We should do somthing here
			}
#pragma warning restore CS0168 // The variable 'ex' is declared but never used

			return decypted;
			}
		}

		private ICryptoTransform GetCryptoTransform(AesCryptoServiceProvider csp, bool encrypting)
		{
						
			try
			{
				csp.Mode = CipherMode.CBC;
				csp.Padding = PaddingMode.PKCS7;

				byte[] salt = Encoding.UTF8.GetBytes(Globals.Salt);

				String iv = Globals.Cypher1;

				var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Globals.Passcode.ToCharArray()), salt, 65536);

				byte[] key = spec.GetBytes(16);


				csp.IV = Encoding.UTF8.GetBytes(iv);
				csp.Key = key;

				if (encrypting)
				{
					return csp.CreateEncryptor();
				}


			}

#pragma warning disable CS0168 // The variable 'ex' is declared but never used
			catch (Exception ex)
			{
				// We should do somthing here
			}
#pragma warning restore CS0168 // The variable 'ex' is declared but never used

			return csp.CreateDecryptor();
		}
	}
}
