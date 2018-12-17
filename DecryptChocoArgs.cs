using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

//=======================================================================//
//
// Author:				Collin Chaffin
// Last Modified:		12-17-2018_05:07AM
// Filename:			DecryptChocoArgs.cs
//
//
// Changelog:
//
//	v1.0	:	12-17-2018	:	Initial release
//  v1.0.1	:	12-17-2018	:	Add additional output text/formatting
//
// Examples:
//  Decrypt argument file in current directory:
//   C:\ProgramData\chocolatey\.chocolatey\package123> DecryptChocoArgs .\.arguments
//
//   Decrypted Chocolately Arguments:
//
//   --prerelease --install-arguments="'SOME_VARIABLE=VALUE'" --allow-downgrade
//
//
// TODO:
//
//  -Implement output for saving current args as reference?
//  -Implement conversion for editing purposes (spin-off util)?
//
//=======================================================================//

namespace DecryptChocoArgs
{
    public class Program
    {
        private const string Usage = "Usage:  DecryptChocoArgs \"path-to-choco-arguments-file\"";
		
		private static readonly byte[] entropyBytes = Encoding.UTF8.GetBytes("Chocolatey");
        
        public static void Main(string[] args)
        {
			try
			{
				if (args.Length == 0 || args.Length > 1){
					throw new Exception("\nChocolately argument file not provided.\n\n" + Usage);
				}
					
				if (File.Exists(args[0])) {
					
					string fileName = args[0];
					
					var encryptedBytes = Convert.FromBase64String(File.ReadAllText(fileName));
					
					var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, DataProtectionScope.LocalMachine);
					
					string decryptedChocoArgs = Encoding.UTF8.GetString(decryptedBytes);

					Console.WriteLine("\nDecrypted Chocolately Arguments:\n\n" + decryptedChocoArgs);
				}
				else {
					throw new FileNotFoundException();
				}			
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("\nChocolately argument file not found.\n\n" + Usage);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
    }
}
