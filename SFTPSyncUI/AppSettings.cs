
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFTPSyncUI
{
    public class AppSettings
    {
        private static string DefaultSettingsFile = Path.Combine(Path.GetDirectoryName(SFTPSyncUI.ExecutableFile) ?? "", "appsettings.json");
        private static string SettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SFTPSync.json");
        private static bool initialLoadSettings = true;

        public static AppSettings? LoadFromFile()
        {
            AppSettings? settings = null;

            // Do we have a user settings file?
            bool justCreated = false;
            bool pathsRepaired = false;

            if (!File.Exists(SettingsFile))
            {
                try
                {
                    File.Copy(DefaultSettingsFile, SettingsFile);
                    justCreated = true;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Failed to create user settings file: {ex.Message}");
                }
            }

            // Load the settings file

            try
            {
                string jsonString = File.ReadAllText(SettingsFile);

                // Fix single backslashes in hand-edited Windows paths before the JSON is parsed
                jsonString = RepairPathBackslashes(jsonString, out pathsRepaired);

                settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                if (settings != null)
                {
                    initialLoadSettings = false;
                    if (justCreated)
                    {
                        settings.SaveToFile();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to load settings: {ex.Message}");
            }

            // Verify settings
            if (settings != null)
            {
                var setingsChanged = pathsRepaired;

                // Exclusions are stored relative to the local path. Validate them on every load (a user may have
                // hand-edited the file) and convert full paths from older settings files. This uses the configured
                // local path before it is checked for existence below, as the conversion is purely textual.
                var normalizedExclusions = NormalizeExcludedDirectories(settings.LocalPath, settings.ExcludedDirectories);
                if (normalizedExclusions.Errors.Count > 0)
                {
                    throw new InvalidSettingsException(
                        "The excluded directories in the settings file are not valid:\n\n"
                        + string.Join("\n", normalizedExclusions.Errors.Select(e => " - " + e))
                        + "\n\nExcluded directories must be subdirectories of the Local Path, stored as paths relative to it."
                        + $"\n\nPlease correct the settings file and start the application again:\n{SettingsFile}");
                }

                if (!normalizedExclusions.Directories.SequenceEqual(settings.ExcludedDirectories, StringComparer.Ordinal))
                {
                    settings.excludedDirectories = normalizedExclusions.Directories;
                    setingsChanged = true;
                }

                if (!string.IsNullOrWhiteSpace(settings.LocalPath)
                    && !Directory.Exists(settings.LocalPath))
                {
                    settings.LocalPath = String.Empty;
                    setingsChanged = true;
                }

                // Remove excluded directories that no longer exist (only possible to check with a valid local path)
                if (!string.IsNullOrWhiteSpace(settings.LocalPath)
                    && settings.excludedDirectories.RemoveAll(dir => !Directory.Exists(Path.Combine(settings.LocalPath, dir))) > 0)
                {
                    setingsChanged = true;
                }

                if (settings.AccessVerified &&
                    (string.IsNullOrWhiteSpace(settings.RemoteHost) ||
                     string.IsNullOrWhiteSpace(settings.RemoteUsername) ||
                     string.IsNullOrWhiteSpace(settings.RemotePassword)))
                {
                    settings.AccessVerified = false;
                    setingsChanged = true;
                }

                if (setingsChanged)
                {
                    settings.SaveToFile();
                }

            }

            return settings ?? null;
        }

        /// <summary>
        /// Fix unescaped backslashes in the Windows path values (LocalPath and ExcludedDirectories) of the raw JSON text.
        /// A path pasted into the file by hand has single backslashes (C:\dev\proj), which JSON requires to be doubled.
        /// This must run before deserializing: sequences such as \d make the JSON invalid, and others (\t \n \r \b \f)
        /// are valid escapes that would silently turn into control characters. Those are never valid in a path, so
        /// they are treated as a backslash followed by a letter.
        /// </summary>
        internal static string RepairPathBackslashes(string json, out bool changed)
        {
            var sb = new StringBuilder(json.Length + 16);
            string? currentKey = null;
            bool inExcludedArray = false;
            char previous = '\0';   // the previous significant character outside of strings ('"' after a string)
            int i = 0;

            while (i < json.Length)
            {
                char c = json[i];

                if (c == '"')
                {
                    bool isValue = previous == ':' || (inExcludedArray && (previous == '[' || previous == ','));
                    bool isPath = isValue && (inExcludedArray || currentKey == "LocalPath");

                    int start = i;
                    int j = i + 1;

                    if (isPath)
                    {
                        sb.Append('"');
                        while (j < json.Length)
                        {
                            char ch = json[j];

                            if (ch == '"')
                                break;

                            if (ch != '\\')
                            {
                                sb.Append(ch);
                                j++;
                                continue;
                            }

                            char next = j + 1 < json.Length ? json[j + 1] : '\0';

                            if (next == '\\' || next == '/')
                            {
                                // Already escaped
                                sb.Append(ch).Append(next);
                                j += 2;
                            }
                            else if (next == 'u' && j + 5 < json.Length && json.Substring(j + 2, 4).All(Uri.IsHexDigit))
                            {
                                // Unicode escape
                                sb.Append(json, j, 6);
                                j += 6;
                            }
                            else if (next == '"' && IsEndOfValue(json, j + 2))
                            {
                                // A path ending in a backslash: the quote closes the string
                                sb.Append(@"\\");
                                j++;
                            }
                            else if (next == '"')
                            {
                                // Genuinely escaped quote; leave it alone
                                sb.Append(ch).Append(next);
                                j += 2;
                            }
                            else
                            {
                                // A lone backslash (including \b \f \n \r \t, which can't be in a path)
                                sb.Append(@"\\");
                                j++;
                            }
                        }

                        if (j < json.Length)
                            sb.Append('"');
                    }
                    else
                    {
                        while (j < json.Length && json[j] != '"')
                        {
                            if (json[j] == '\\')
                                j++;
                            j++;
                        }

                        sb.Append(json, start, Math.Min(j + 1, json.Length) - start);

                        // A string that is not a value is a key
                        if (!isValue)
                            currentKey = json.Substring(start + 1, Math.Min(j, json.Length) - start - 1);
                    }

                    i = j + 1;
                    previous = '"';
                    continue;
                }

                sb.Append(c);
                i++;

                if (char.IsWhiteSpace(c))
                    continue;

                previous = c;

                if (c == '[' && currentKey == "ExcludedDirectories")
                    inExcludedArray = true;
                else if (c == ']')
                    inExcludedArray = false;
            }

            var result = sb.ToString();
            changed = !string.Equals(result, json, StringComparison.Ordinal);
            return result;
        }

        // True if only whitespace follows before a ',' ']' '}' or the end of the text
        private static bool IsEndOfValue(string json, int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;

            return index >= json.Length || json[index] == ',' || json[index] == ']' || json[index] == '}';
        }

        // Path.GetInvalidPathChars() only reports '|' and control characters on modern .NET, so list the rest explicitly
        private static bool HasInvalidPathCharacters(string path) =>
            path.IndexOfAny(['<', '>', '"', '|', '?', '*']) >= 0
            || path.Any(char.IsControl)
            || (!Path.IsPathRooted(path) && path.Contains(':'));

        /// <summary>
        /// Try to express a directory as a path relative to the local path. The directory may be a full path or already
        /// relative to the local path. Fails if it is not a strict subdirectory of the local path.
        /// </summary>
        public static bool TryGetRelativeSubdirectory(string localPath, string directory, out string relativePath)
        {
            relativePath = String.Empty;

            if (string.IsNullOrWhiteSpace(localPath) || string.IsNullOrWhiteSpace(directory)
                || HasInvalidPathCharacters(directory))
            {
                return false;
            }

            try
            {
                var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(localPath));

                // A relative path must not use ".." to escape (or wander around in) the local path
                if (!Path.IsPathRooted(directory)
                    && directory.Split('\\', '/').Any(segment => segment == ".."))
                {
                    return false;
                }

                var full = Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(Path.IsPathRooted(directory) ? directory : Path.Combine(root, directory)));

                var relative = Path.GetRelativePath(root, full);

                // Same directory, outside the local path, or on a different drive
                if (relative == "."
                    || Path.IsPathRooted(relative)
                    || relative == ".."
                    || relative.StartsWith(".." + Path.DirectorySeparatorChar))
                {
                    return false;
                }

                relativePath = relative;
                return true;
            }
            catch (Exception)
            {
                // Invalid characters or otherwise malformed path
                return false;
            }
        }

        /// <summary>
        /// Validate a list of excluded directories against the local path, and convert any full paths to relative ones.
        /// Relative paths are kept when there is no local path (they are checked structurally only), but a full path
        /// cannot be converted without one.
        /// </summary>
        private static (List<string> Directories, List<string> Errors) NormalizeExcludedDirectories(string localPath, List<string> directories)
        {
            var result = new List<string>();
            var errors = new List<string>();

            foreach (var directory in directories)
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    errors.Add("(blank entry)");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(localPath))
                {
                    // Nothing to compare with. A full path can't be made relative; a relative one must at least be well formed.
                    bool wellFormedRelative = !Path.IsPathRooted(directory)
                        && !HasInvalidPathCharacters(directory)
                        && !directory.Split('\\', '/').Any(segment => segment == "..");

                    if (wellFormedRelative)
                        result.Add(directory);
                    else
                        errors.Add($"{directory} (no Local Path is set)");

                    continue;
                }

                if (TryGetRelativeSubdirectory(localPath, directory, out var relative))
                {
                    if (!result.Contains(relative, StringComparer.OrdinalIgnoreCase))
                        result.Add(relative);
                }
                else
                {
                    errors.Add($"{directory} (not a subdirectory of {localPath})");
                }
            }

            return (result, errors);
        }

        /// <summary>
        /// The excluded directories as full paths (the local path combined with each relative path).
        /// </summary>
        public List<string> GetExcludedDirectoryFullPaths()
        {
            if (string.IsNullOrWhiteSpace(LocalPath))
                return new List<string>();

            return ExcludedDirectories
                .Select(relative => Path.GetFullPath(Path.Combine(LocalPath, relative)))
                .ToList();
        }

        public bool SaveToFile()
        {
            bool saved = false;

            if (initialLoadSettings == true)
                return false;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsFile, json);
                saved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return saved;
        }

        // Run at login

        private bool startAtLogin = false;
        public bool StartAtLogin
        {
            get => startAtLogin;
            set
            {
                if (startAtLogin != value)
                {
                    startAtLogin = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        // Start in tray

        private bool startInTray = false;
        public bool StartInTray
        {
            get => startInTray;
            set
            {
                if (startInTray != value)
                {
                    startInTray = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        // Auto start sync

        private bool autoStartSync = false;
        public bool AutoStartSync
        {
            get => autoStartSync;
            set
            {
                if (autoStartSync != value)
                {
                    autoStartSync = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        // Access verified

        private bool accessVerified = false;
        public bool AccessVerified
        {
            get => accessVerified;
            set
            {
                if (accessVerified != value)
                {
                    accessVerified = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        // Support delted files and folders

        private bool deleteEnabled = false;
        public bool DeleteEnabled
        {
            get => deleteEnabled;
            set
            {
                if (deleteEnabled != value)
                {
                    deleteEnabled = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Local path

        private string localPath = String.Empty;

        public string LocalPath
        {
            get => localPath;
            set
            {
                if (localPath != value)
                {
                    localPath = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Local search pattern

        private string localSearchPattern = String.Empty;

        public string LocalSearchPattern
        {
            get => localSearchPattern;
            set
            {
                if (localSearchPattern != value)
                {
                    localSearchPattern = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Remote host

        private string remoteHost = String.Empty;

        public string RemoteHost
        {
            get => remoteHost;
            set
            {
                if (remoteHost != value)
                {
                    remoteHost = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Remote username

        private string remoteUsername = String.Empty;

        public string RemoteUsername
        {
            get => remoteUsername;
            set
            {
                if (remoteUsername != value)
                {
                    remoteUsername = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Remote password

        private string remotePassword = String.Empty;

        public string RemotePassword
        {
            get => remotePassword;
            set
            {
                if (remotePassword != value)
                {
                    remotePassword = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Remote path

        private string remotePath = String.Empty;

        public string RemotePath
        {
            get => remotePath;
            set
            {
                if (remotePath != value)
                {
                    remotePath = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }

        //Excluded directories, stored as paths relative to LocalPath

        private List<string> excludedDirectories = new List<string>();

        public List<string> ExcludedDirectories
        {
            get => excludedDirectories;
            set
            {
                if (excludedDirectories != value)
                {
                    excludedDirectories = value;
                    if (!initialLoadSettings)
                    {
                        SaveToFile();
                    }
                }
            }
        }
    }
    /// <summary>
    /// The settings file contains values that make it unsafe to continue (for example invalid excluded directories).
    /// </summary>
    public class InvalidSettingsException : ApplicationException
    {
        public InvalidSettingsException(string message) : base(message) { }
    }
}
