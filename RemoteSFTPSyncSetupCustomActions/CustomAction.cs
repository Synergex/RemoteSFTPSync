
using Microsoft.Deployment.WindowsInstaller;
using System;
using System.IO;

namespace RemoteSFTPSyncSetupCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ReplaceTokens(Session session)
        {
            System.Diagnostics.Debugger.Launch();

            try
            {
                string installFolder = session.CustomActionData["INSTALLFOLDER"];
                string localRoot = session.CustomActionData["LOCAL_ROOT"];
                string localSearchPattern = session.CustomActionData["LOCAL_SEARCH_PATTERN"];
                string remoteHost = session.CustomActionData["REMOTE_HOST"];
                string remoteUser = session.CustomActionData["REMOTE_USER"];
                string remotePassword = session.CustomActionData["REMOTE_PASSWORD"];
                string remotePath = session.CustomActionData["REMOTE_PATH"];

                if (string.IsNullOrWhiteSpace(installFolder))
                {
                    session.Log("ERROR: INSTALLFOLDER is empty!");
                    return ActionResult.Failure;
                }

                if (!Directory.Exists(installFolder))
                {
                    session.Log("ERROR: INSTALLFOLDER does not exist!");
                    return ActionResult.Failure;
                }

                if (!File.Exists(Path.Combine(installFolder, "appsettings.json")))
                {
                    session.Log("ERROR: appsettings.json does not exist in INSTALLFOLDER!");
                    return ActionResult.Failure;
                }

                if (string.IsNullOrWhiteSpace(localRoot))
                {
                    session.Log("ERROR: LOCAL_ROOT is empty!");
                    return ActionResult.Failure;
                }

                if (!Directory.Exists(localRoot))
                {
                    session.Log("ERROR: LOCAL_ROOT does not exist!");
                    return ActionResult.Failure;
                }

                if (string.IsNullOrWhiteSpace(localSearchPattern))
                {
                    localSearchPattern = "*.*";
                }

                if (string.IsNullOrWhiteSpace(remoteHost))
                {
                    session.Log("ERROR: REMOTE_HOST is empty!");
                    return ActionResult.Failure;
                }

                if (string.IsNullOrWhiteSpace(remoteUser))
                {
                    session.Log("ERROR: REMOTE_USER is empty!");
                    return ActionResult.Failure;
                }

                if (string.IsNullOrWhiteSpace(remotePassword))
                {
                    session.Log("ERROR: REMOTE_PASSWORD is empty!");
                    return ActionResult.Failure;
                }

                if (string.IsNullOrWhiteSpace(remotePath))
                {
                    session.Log("ERROR: REMOTE_PATH is empty!");
                    return ActionResult.Failure;
                }

                string configTemplatePath = Path.Combine(installFolder, "appsettings.json");

                // Do your replacement logic
                var content = File.ReadAllText(configTemplatePath)
                                  .Replace("[LOCAL_ROOT]", localRoot)
                                  .Replace("[LOCAL_SEARCH_PATTERN]", localSearchPattern)
                                  .Replace("[REMOTE_HOST]", remoteHost)
                                  .Replace("[REMOTE_USER]", remoteUser)
                                  .Replace("[REMOTE_PASSWORD]", remotePassword)
                                  .Replace("[REMOTE_PATH]", remotePath);

                string configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "RemoteSFTPSync.config");
                
                File.WriteAllText(configPath, content);

                return ActionResult.Success;
            }
            catch (System.Exception ex)
            {
                session.Log("ERROR: Custom action failed: " + ex.ToString());
                return ActionResult.Failure;
            }
        }
    }
}
